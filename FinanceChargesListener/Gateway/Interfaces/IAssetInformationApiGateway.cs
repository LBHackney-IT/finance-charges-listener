using System;
using System.Threading.Tasks;
using Hackney.Shared.Asset.Domain;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface IAssetInformationApiGateway
    {
        Task<Asset> GetAssetEstimateById(Guid id);
    }
}
