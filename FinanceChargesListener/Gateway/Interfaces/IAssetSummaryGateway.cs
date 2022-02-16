using FinanceChargesListener.Domain;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface IAssetSummaryGateway
    {
        Task<AssetSummary> GetAssetSummaryByAssetIdAsync(Guid assetId);

        Task<AssetSummary> UpdateAssetSummaryAsync(Guid assetId, decimal totalServiceCharges);
    }
}
