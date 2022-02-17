using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using FinanceChargesListener.Boundary;
using FinanceChargesListener.Common;
using FinanceChargesListener.Domain.EventMessages;
using FinanceChargesListener.Gateway;
using FinanceChargesListener.Gateway.Extensions;
using FinanceChargesListener.Infrastructure;
using FinanceChargesListener.Infrastructure.Interfaces;
using FinanceChargesListener.UseCase;
using FinanceChargesListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ApplyHeadOfChargeUseCase = FinanceChargesListener.UseCase.Interfaces.ApplyHeadOfChargeUseCase;
using IAssetInformationApiGateway = FinanceChargesListener.Gateway.Services.Interfaces.IAssetInformationApiGateway;
using IChargesApiGateway = FinanceChargesListener.Gateway.Interfaces.IChargesApiGateway;
using ChargesMaintenanceApiGateway = FinanceChargesListener.Gateway.Interfaces.ChargesMaintenanceApiGateway;
using HousingSearchService = FinanceChargesListener.Gateway.Services.Interfaces.HousingSearchService;
using FinanceChargesListener.Gateway.Services;
using FinanceChargesListener.Gateway.Interfaces;

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
            services.AddScoped<IChargesApiGateway, Gateway.ChargesApiGateway>();
            services.AddScoped<ChargesMaintenanceApiGateway, Gateway.ChargesMaintenanceApiGateway>();

            services.ConfigureAws();
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            services.AddScoped<ApplyHeadOfChargeUseCase, UseCase.ApplyHeadOfChargeUseCase>();
            services.AddScoped<IEstimateActualFileProcessUseCase, EstimateActualFileProcessUseCase>();
            services.AddScoped<IManagementFeeUseCase, ManagementFeeUseCase>();
            services.AddScoped<ICommonMethodUseCase, CommonMethodUseCase>();
            services.AddScoped<IProcessTenantsChargesUseCase, ProcessTenantsChargesUseCase>();
            services.AddScoped<IProcessLeaseholdChargesUseCase, ProcessLeaseholdChargesUseCase>();
            services.AddScoped<IUpdateChargesUseCase, UpdateChargesUseCase>();

            services.AddAmazonS3(Configuration);

            services.AddHttpContextAccessor();
            RegisterGateways(services);
            base.ConfigureServices(services);
        }

        private static void RegisterGateways(IServiceCollection services)
        {
            services.AddTransient<LoggingDelegatingHandler>();
            services.AddScoped<Gateway.Interfaces.AssetGateway, Gateway.AssetGateway>();

            var housingSearchApiUrl = Environment.GetEnvironmentVariable("HOUSING_SEARCH_API_URL");
            var housingSearchApiToken = Environment.GetEnvironmentVariable("HOUSING_SEARCH_API_TOKEN");

            services.AddHttpClient<HousingSearchService, Gateway.Services.HousingSearchService>(c =>
            {
                c.BaseAddress = new Uri(housingSearchApiUrl);
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(housingSearchApiToken);
            })
           .AddHttpMessageHandler<LoggingDelegatingHandler>();

            var assetInformationApiUrl = Environment.GetEnvironmentVariable("ASSET_INFORMATION_API_URL");
            var assetInformationApiToken = Environment.GetEnvironmentVariable("ASSET_INFORMATION_API_TOKEN");
            services.AddHttpClient<IAssetInformationApiGateway, AssetInformationApiGateway>(c =>
            {
                c.BaseAddress = new Uri(assetInformationApiUrl);
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(assetInformationApiToken);
            })
            .AddHttpMessageHandler<LoggingDelegatingHandler>();

            services.AddHttpClient<IFinancialSummaryApiGateway, FinancialSummaryApiGateway>()
                .AddHttpMessageHandler<LoggingDelegatingHandler>();
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

            
            var eventJson = JsonSerializer.Serialize(eventModel, JsonOptions);

            EntityEventSns entityEvent = JsonSerializer.Deserialize<ChargesEventSns>(eventJson, JsonOptions);
            using (Logger.BeginScope("CorrelationId: {CorrelationId}", entityEvent.CorrelationId))
            {
                try
                {
                    IMessageProcessing processor = entityEvent.EventType switch
                    {
                        EventTypes.HeadOfChargeApplyEvent => ServiceProvider.GetService<ApplyHeadOfChargeUseCase>(),
                        EventTypes.FileUploadEvent => ServiceProvider.GetService<IEstimateActualFileProcessUseCase>(),
                        EventTypes.DwellingChargeUpdatedEvent => ServiceProvider.GetService<IUpdateChargesUseCase>(),
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
