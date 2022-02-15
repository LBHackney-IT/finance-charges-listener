using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Infrastructure.Interfaces;
using Hackney.Shared.Asset.Domain;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class AssetGateway : IAssetGateway
    {
        private readonly IHousingData<Asset> _housingData;

        public AssetGateway(IHousingData<Asset> housingData)
        {
            _housingData = housingData;
        }

        public async Task<Asset> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(id)} shouldn't be empty.");
            }

            return await _housingData.DownloadAsync(id).ConfigureAwait(false);
        }
    }
}
