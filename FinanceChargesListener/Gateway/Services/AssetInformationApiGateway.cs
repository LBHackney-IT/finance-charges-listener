using FinanceChargesListener.Gateway.Extensions;
using Hackney.Shared.Asset.Domain;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Services
{
    public class AssetInformationApiGateway : Interfaces.AssetInformationApiGateway
    {
        private readonly HttpClient _client;

        public AssetInformationApiGateway(HttpClient client)
        {
            _client = client;
        }

        public async Task<Asset> GetAssetByAssetIdAsync(string assetId)
        {
            var uri = new Uri($"api/v1/assets/assetId/{assetId}", UriKind.Relative);

            var response = await _client.GetAsync(uri).ConfigureAwait(true);
            var result = await response.ReadContentAs<Asset>().ConfigureAwait(true);
            return result;
        }

        public async Task<Asset> GetAssetByIdAsync(Guid assetId)
        {
            var uri = new Uri($"api/v1/assets/{assetId}", UriKind.Relative);

            var response = await _client.GetAsync(uri).ConfigureAwait(true);
            var result = await response.ReadContentAs<Asset>().ConfigureAwait(true);
            return result;
        }
    }
}
