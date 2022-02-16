using FinanceChargesListener.Domain;
using FinanceChargesListener.Infrastructure.Interfaces;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Infrastructure
{
    public class UpdateAssetSummary : IUpdateAssetSummary
    {
        public Task<AssetSummary> UpdateAsync(Guid assetIwd, decimal totalServiceCharges)
        {
            throw new NotImplementedException();
        }
    }
}
