using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using FinanceChargesListener.Boundary;
using FinanceChargesListener.Common;
using FinanceChargesListener.Gateway;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Gateway.Services;
using FinanceChargesListener.Gateway.Services.Interfaces;
using FinanceChargesListener.Infrastructure;
using FinanceChargesListener.UseCase;
using FinanceChargesListener.UseCase.Interfaces;
using Hackney.Core.DynamoDb;
using Hackney.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ApplyHeadOfChargeUseCase = FinanceChargesListener.UseCase.Interfaces.ApplyHeadOfChargeUseCase;
using AssetInformationApiGateway = FinanceChargesListener.Gateway.Services.Interfaces.AssetInformationApiGateway;
using ChargesApiGateway = FinanceChargesListener.Gateway.Interfaces.ChargesApiGateway;
using ChargesMaintenanceApiGateway = FinanceChargesListener.Gateway.Interfaces.ChargesMaintenanceApiGateway;
using EsGateway = FinanceChargesListener.Gateway.Interfaces.EsGateway;
using HousingSearchService = FinanceChargesListener.Gateway.Services.Interfaces.HousingSearchService;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FinanceChargesListener
{
    /// <summary>
    /// Lambda function triggered by an SQS message
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ChargesListener : BaseFunction
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public ChargesListener()
        { }

        /// <summary>
        /// Use this method to perform any DI configuration required
        /// </summary>
        /// <param name="services"></param>
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddScoped<ChargesApiGateway, Gateway.ChargesApiGateway>();
            services.AddScoped<ChargesMaintenanceApiGateway, Gateway.ChargesMaintenanceApiGateway>();

            services.ConfigureAws();

            services.AddScoped<ApplyHeadOfChargeUseCase, UseCase.ApplyHeadOfChargeUseCase>();
            services.AddScoped<IManagementFeeUseCase, ManagementFeeUseCase>();
            services.AddScoped<ICommonMethodUseCase, CommonMethodUseCase>();
            services.AddScoped<IProcessTenantsChargesUseCase, ProcessTenantsChargesUseCase>();
            services.AddScoped<IProcessLeaseholdChargesUseCase, ProcessLeaseholdChargesUseCase>();


            services.AddScoped<DbEntityGateway, DynamoDbEntityGateway>();

            RegisterGateways(services);
            base.ConfigureServices(services);
        }

        private static void RegisterGateways(IServiceCollection services)
        {
            services.AddTransient<LoggingDelegatingHandler>();
            services.AddScoped<Gateway.Interfaces.AssetGateway, Gateway.AssetGateway>();

            var housingSearchApiUrl = Environment.GetEnvironmentVariable("HOUSING_SEARCH_API_URL");
            //var housingSearchApiKey = Environment.GetEnvironmentVariable("HOUSING_SEARCH_API_KEY");
            var housingSearchApiToken = Environment.GetEnvironmentVariable("HOUSING_SEARCH_API_TOKEN");

           
            services.AddHttpClient<HousingSearchService, Gateway.Services.HousingSearchService>(c =>
            {
                c.BaseAddress = new Uri(housingSearchApiUrl);
                //c.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", housingSearchApiKey);
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(housingSearchApiToken);
            })
           .AddHttpMessageHandler<LoggingDelegatingHandler>()
           .AddPolicyHandler(GetRetryPolicy())
           .AddPolicyHandler(GetCircuitBreakerPolicy());


            var assetInformationApiUrl = Environment.GetEnvironmentVariable("ASSET_INFORMATION_API_URL");
            var assetInformationApiToken = Environment.GetEnvironmentVariable("ASSET_INFORMATION_API_TOKEN");
            services.AddHttpClient<AssetInformationApiGateway, Gateway.Services.AssetInformationApiGateway>(c =>
            {
                c.BaseAddress = new Uri(assetInformationApiUrl);
                //c.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", housingSearchApiKey);
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(assetInformationApiToken);
            })
               .AddHttpMessageHandler<LoggingDelegatingHandler>();
        }
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // In this case will wait for
            //  2 ^ 1 = 2 seconds then
            //  2 ^ 2 = 4 seconds then
            //  2 ^ 3 = 8 seconds then
            //  2 ^ 4 = 16 seconds then
            //  2 ^ 5 = 32 seconds

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, retryCount, context) =>
                    {
                        Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
                    });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            // Do this in parallel???
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Method called to process every distinct message received.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [LogCall(LogLevel.Information)]
        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processing message {message.MessageId}");

            var entityEvent = JsonSerializer.Deserialize<EntityEventSns>(message.Body, JsonOptions);
            using (Logger.BeginScope("CorrelationId: {CorrelationId}", entityEvent.CorrelationId))
            {
                try
                {
                    MessageProcessing processor = entityEvent.EventType switch
                    {
                        EventTypes.HeadOfChargeApplyEvent => ServiceProvider.GetService<ApplyHeadOfChargeUseCase>(),
                        _ => throw new ArgumentException(
                            $"Unknown event type: {entityEvent.EventType} on message id: {message.MessageId}")
                    };

                    await processor.ProcessMessageAsync(entityEvent, JsonOptions).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Exception processing message id: {message.MessageId}; type: {entityEvent.EventType}; entity id: {entityEvent.EntityId}");
                    throw; // AWS will handle retry/moving to the dead letter queue
                }
            }
        }
    }
}
