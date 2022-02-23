using FinanceChargesListener.Domain;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Infrastructure.Interfaces
{
    public interface IFinancialSummaryApiGateway
    {
        Task<AssetEstimateSummary> GetAssetEstimate(Guid assetId, short chargeYear, string valuesType);

        Task UpdateTotalServiceCharges(Guid assetId, decimal newTotalServiceCharges, short summaryYear, string valuesType);

        Task<bool> AddEstimateSummary(AddAssetSummaryRequest addAssetSummaryRequest);
    }
}
