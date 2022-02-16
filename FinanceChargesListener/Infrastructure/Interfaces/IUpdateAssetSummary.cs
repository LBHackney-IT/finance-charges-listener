using FinanceChargesListener.Domain;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Infrastructure.Interfaces
{
    public interface IUpdateAssetSummary
    {
        Task<AssetEstimateSummary> UpdateAsync(Guid assetId, decimal totalServiceCharges);
    }
}
