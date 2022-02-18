using Hackney.Shared.Asset.Domain;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Services.Interfaces
{
    public interface AssetInformationApiGateway
    {
        Task<Asset> GetAssetByIdAsync(Guid assetId);

        Task<Asset> GetAssetByAssetIdAsync(string assetId);
    }
}
