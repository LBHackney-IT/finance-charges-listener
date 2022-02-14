using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Services.Interfaces
{
    public interface IFinancialSummaryService
    {
        Task<bool> AddEstimateSummary(AddAssetSummaryRequest addAssetSummaryRequest);
    }
}
