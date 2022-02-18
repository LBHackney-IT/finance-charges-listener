using System.Collections.Generic;
using FinanceChargesListener.Domain;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Services.Interfaces
{
    public interface IFinancialSummaryService
    {
        Task<bool> AddEstimateSummary(AddAssetSummaryRequest addAssetSummaryRequest);
        Task<bool> AddEstimateActualSummaryBatch(IEnumerable<AddAssetSummaryRequest> addAssetSummariesRequest);
    }
}
