using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FinanceChargesListener.Tests
{
    /// <summary>
    /// Class used to set up a real local DynamoDb instance so that it can be used by Gateway and E2E tests
    /// </summary>
    public class AwsIntegrationTest
    {
        public IDynamoDBContext DynamoDbContext => _factory?.DynamoDbContext;
        public IAmazonDynamoDB DynamoDb => _factory?.DynamoDb;

        private readonly AwsMockApplicationFactory _factory;
        private readonly IHost _host;
        private readonly List<TableDef> _tables = new List<TableDef>
        {
            new TableDef()
            {
                TableName = "Charges",
                PartitionKey = new AttributeDef()
                {
                    KeyName = "target_id",
                    KeyType = KeyType.HASH,
                    KeyScalarType = ScalarAttributeType.S
                },
                RangeKey = new AttributeDef()
                {
                    KeyName = "id",
                    KeyType = KeyType.RANGE,
                    KeyScalarType = ScalarAttributeType.S
                }
            },
        };

        public AwsIntegrationTest()
        {
            EnsureEnvVarConfigured("DynamoDb_LocalMode", "true");
            EnsureEnvVarConfigured("DynamoDb_LocalServiceUrl", "http://localhost:8000");
            EnsureEnvVarConfigured("DynamoDb_LocalSecretKey", "8ksq4m6");
            EnsureEnvVarConfigured("DynamoDb_LocalAccessKey", "ig7pb");

            _factory = new AwsMockApplicationFactory(_tables);

            _host = _factory.CreateHostBuilder(null).Build();
            _host.Start();


            LogCallAspectFixture.SetupLogCallAspect();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != _host)
                {
                    _host.StopAsync().GetAwaiter().GetResult();
                    _host.Dispose();
                }
                _disposed = true;
            }
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }


    }

    public class TableDef
    {
        public string TableName { get; set; }
        public AttributeDef PartitionKey { get; set; }

        public AttributeDef RangeKey { get; set; }
    }

    public class AttributeDef
    {
        public string KeyName { get; set; }
        public ScalarAttributeType KeyScalarType { get; set; }
        public KeyType KeyType { get; set; }
    }

    public class GlobalIndexDef : AttributeDef
    {
        public string IndexName { get; set; }
        public string ProjectionType { get; set; }
    }

    [CollectionDefinition("Aws collection", DisableParallelization = true)]
    public class DynamoDbCollection : ICollectionFixture<AwsIntegrationTest>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
