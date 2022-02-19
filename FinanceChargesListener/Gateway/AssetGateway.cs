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

        public async Task<AssetPaginationResponse> GetAll(int count, Dictionary<string, AttributeValue> lastEvaluatedKey = null)
        {
            try
            {
                ScanRequest request = new ScanRequest("Assets")
                {
                    Limit = count,
                    ExclusiveStartKey = lastEvaluatedKey
                };
                ScanResponse response = await _dynamoDb.ScanAsync(request).ConfigureAwait(false);
                if (response == null || response.Items == null)
                    throw new Exception($"_dynamoDb.ScanAsync results NULL: {response?.ToString()}");

                return new AssetPaginationResponse()
                {
                    LastKey = response?.LastEvaluatedKey,
                    Assets = response?.ToAssets()?.ToList()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
