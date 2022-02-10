using System;
using System.Threading.Tasks;
using FinanceChargesListener.Domain;
using Hackney.Shared.Asset.Domain;

namespace FinanceChargesListener.Gateway.Services.Interfaces
{
    public interface AssetInformationApiGateway
    {
        Task<Asset> GetAssetByIdAsync(Guid assetId);
    }
}
