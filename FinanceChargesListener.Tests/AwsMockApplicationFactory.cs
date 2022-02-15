using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using Hackney.Core.DynamoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace FinanceChargesListener.Tests
{
    public class AwsMockApplicationFactory
    {
        private readonly List<TableDef> _tables;

        public IAmazonDynamoDB DynamoDb { get; private set; }
        public IDynamoDBContext DynamoDbContext { get; private set; }


        public AwsMockApplicationFactory(List<TableDef> tables)
        {
            _tables = tables;
        }

        public IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
           .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
           .ConfigureServices((hostContext, services) =>
           {
               services.ConfigureDynamoDB();
               services.AddHttpClient();

               var serviceProvider = services.BuildServiceProvider();
               DynamoDb = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
               DynamoDbContext = serviceProvider.GetRequiredService<IDynamoDBContext>();
               EnsureTablesExist(DynamoDb, _tables);

           });

        private static void EnsureTablesExist(IAmazonDynamoDB dynamoDb, List<TableDef> tables)
        {
            foreach (var table in tables)
            {
                try
                {
                    // Hanna Holosova
                    // This command helps to prevent the next exception:
                    // Amazon.XRay.Recorder.Core.Exceptions.EntityNotAvailableException : Entity doesn't exist in AsyncLocal
                    AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;

                    List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();
                    List<GlobalSecondaryIndex> globalSecondaryIndexes = new List<GlobalSecondaryIndex>();

                    attributeDefinitions.Add(new AttributeDefinition(table.PartitionKey.KeyName,
                        table.PartitionKey.KeyScalarType));



                    CreateTableRequest request = new CreateTableRequest
                    {
                        TableName = table.TableName,
                        ProvisionedThroughput =
                            new ProvisionedThroughput { ReadCapacityUnits = (long) 3, WriteCapacityUnits = (long) 3 },
                        AttributeDefinitions = attributeDefinitions,
                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement(table.PartitionKey.KeyName, table.PartitionKey.KeyType)
                        },
                    };

                    _ = dynamoDb.CreateTableAsync(request).GetAwaiter().GetResult();
                }
                catch (ResourceInUseException)
                {
                    // It already exists :-)
                }
                catch (Exception exception)
                {
                    throw new Exception("Exception in checking table existence", exception);
                }
            }
        }
    }
}
