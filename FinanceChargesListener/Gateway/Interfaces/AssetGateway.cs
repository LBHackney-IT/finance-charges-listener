using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Boundary;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface AssetGateway
    {
        Task<Asset> GetAssetByIdAsync(Guid assetId);
        Task<AssetPaginationResponse> GetAll(int count, Dictionary<string, AttributeValue> lastEvaluatedKey = null);
    }
}
