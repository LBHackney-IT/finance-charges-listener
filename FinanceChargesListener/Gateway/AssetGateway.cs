using Amazon.DynamoDBv2.DataModel;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Factories;
using Hackney.Shared.Asset.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class AssetGateway : Interfaces.AssetGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<AssetGateway> _logger;
        public AssetGateway(IDynamoDBContext dynamoDbContext, ILogger<AssetGateway> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;

        }
        public async Task<Asset> GetAssetByIdAsync(Guid assetId)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for id {assetId}");
            var result = await _dynamoDbContext.LoadAsync<AssetDb>(assetId).ConfigureAwait(true);
            return result?.ToDomain();
        }
    }
}
