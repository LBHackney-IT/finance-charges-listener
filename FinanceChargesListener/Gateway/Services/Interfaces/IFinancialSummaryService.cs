using System.Collections.Generic;
using FinanceChargesListener.Domain;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Services.Interfaces
{
    public interface IFinancialSummaryService
    {
        Task<bool> AddHeadOfChargesSummary(AddAssetSummaryRequest addAssetSummaryRequest);
        Task<bool> AddEstimateActualSummaryBatch(IEnumerable<AddAssetSummaryRequest> addAssetSummariesRequest);
    }
}
