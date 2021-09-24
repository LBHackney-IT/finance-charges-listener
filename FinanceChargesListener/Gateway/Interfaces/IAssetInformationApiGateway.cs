using FinanceChargesListener.Domain;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface IAssetInformationApiGateway
    {
        Task<AssetList> GetEstateBlockList();
        Task<AssetList> GetBlockDetails(Guid id);
    }
}
