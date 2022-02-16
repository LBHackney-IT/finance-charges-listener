using FinanceChargesListener.Domain;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Infrastructure.Interfaces
{
    public interface IUpdateAssetSummary
    {
        Task<AssetSummary> UpdateAsync(Guid assetId, decimal totalServiceCharges);
    }
}
