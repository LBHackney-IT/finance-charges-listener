using FinanceChargesListener.Gateway.Interfaces;
using Hackney.Core.Logging;
using Hackney.Shared.Asset.Domain;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class AssetInformationApiGateway : IAssetInformationApiGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiRoute;

        public AssetInformationApiGateway(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _apiRoute = Environment.GetEnvironmentVariable("ASSET_INFORMATION_API_URL")
                ?? throw new Exception("The ASSET_INFORMATION_API_URL environment variable should be set!");

            var apiToken = Environment.GetEnvironmentVariable("ASSET_INFORMATION_API_TOKEN")
                ?? throw new Exception("The ASSET_INFORMATION_API_TOKEN environment variable should be set!");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        }

        [LogCall]
        public async Task<Asset> GetAssetEstimateById(Guid assetId)
        {
            var uri = new Uri(_apiRoute + "/api/v1/assets/" + assetId);

            var response = await _httpClient.GetAsync(uri).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response == null)
            {
                throw new Exception($"Asset Information API is not reachable.");
            }
            else if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw new Exception($"Unexpected status code from Asset Information API: {response.StatusCode} - {responseBody}");
            }

            var assetEstimate = JsonConvert.DeserializeObject<Asset>(responseBody);

            return assetEstimate;
        }
    }
}
