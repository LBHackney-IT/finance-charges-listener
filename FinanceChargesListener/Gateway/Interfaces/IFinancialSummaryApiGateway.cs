using FinanceChargesListener.Domain;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Infrastructure.Interfaces
{
    public interface IFinancialSummaryApiGateway
    {
        Task<AssetEstimateSummary> GetAssetEstimate(Guid assetId);

        Task UpdateTotalServiceCharges(Guid assetId, decimal newTotalServiceCharges);

        Task<bool> AddEstimateSummary(AddAssetSummaryRequest addAssetSummaryRequest);
    }
}
