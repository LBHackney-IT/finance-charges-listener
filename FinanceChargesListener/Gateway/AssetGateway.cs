using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Boundary;
using FinanceChargesListener.Infrastructure;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class AssetGateway : Interfaces.AssetGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly ILogger<AssetGateway> _logger;
        public AssetGateway(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDB, ILogger<AssetGateway> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _dynamoDb = dynamoDB;
            _logger = logger;

        }
        public async Task<Asset> GetAssetByIdAsync(Guid assetId)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for id {assetId}");
            var result = await _dynamoDbContext.LoadAsync<AssetDb>(assetId).ConfigureAwait(true);
            return result?.ToDomain();
        }

        public async Task<AssetPaginationResponse> GetAllByAssetType(string assetType)
        {
            try
            {
                //ScanRequest request = new ScanRequest("Assets")
                //{
                //    Limit = count,
                //    ExclusiveStartKey = lastEvaluatedKey
                //};
                //ScanResponse response = await _dynamoDb.ScanAsync(request).ConfigureAwait(false);
                //if (response == null || response.Items == null)
                //    throw new Exception($"_dynamoDb.ScanAsync results NULL: {response?.ToString()}");
                var scanRequest = new ScanRequest
                {
                    TableName = "Assets",
                    FilterExpression = "assetType = :assetType",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":assetType", new AttributeValue { S = assetType.ToString() } }
                    }
                };
                var response = await _dynamoDb.ScanAsync(scanRequest).ConfigureAwait(false);
                var responseAssets = response.Items.Select(x => x.GetAssetKeys());
                return new AssetPaginationResponse()
                {
                    Assets = responseAssets?.ToList()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
