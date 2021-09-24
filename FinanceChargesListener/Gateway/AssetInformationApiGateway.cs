using FinanceChargesListener.Domain;
using FinanceChargesListener.Gateway.Interfaces;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class AssetInformationApiGateway : IAssetInformationApiGateway
    {
        public Task<AssetList> GetBlockDetails(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<AssetList> GetEstateBlockList()
        {
            throw new NotImplementedException();
        }
    }
}
