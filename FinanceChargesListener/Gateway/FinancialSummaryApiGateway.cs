using FinanceChargesListener.Domain;
using FinanceChargesListener.Infrastructure.Interfaces;
using Hackney.Core.Logging;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class FinancialSummaryApiGateway : IFinancialSummaryApiGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiRoute;

        public FinancialSummaryApiGateway(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _apiRoute = Environment.GetEnvironmentVariable("FINANCIAL_SUMMARY_API_URL")
                ?? throw new Exception("The FINANCIAL_SUMMARY_API_URL environment variable should be set!");

            var apiToken = Environment.GetEnvironmentVariable("FINANCIAL_SUMMARY_API_TOKEN")
                ?? throw new Exception("The FINANCIAL_SUMMARY_API_TOKEN environment variable should be set!");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        }

        [LogCall]
        public async Task<AssetEstimateSummary> GetAssetEstimate(Guid assetId)
        {
            var uri = new Uri(_apiRoute + "/api/v1/asset-summary/estimates/" + assetId);

            var response = await _httpClient.GetAsync(uri).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response == null)
            {
                throw new Exception($"Financial Summary API is not reachable.");
            }
            else if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw new Exception($"Unexpected status code from Financial Summary API: {response.StatusCode} - {responseBody}");
            }

            var assetEstimate = JsonConvert.DeserializeObject<AssetEstimateSummary>(responseBody);

            return assetEstimate;
        }

        [LogCall]
        public async Task UpdateTotalServiceCharges(Guid assetId, decimal newTotalServiceCharges)
        {
            var uri = new Uri(_apiRoute + "/api/v1/asset-summary/estimates/" + assetId);

            var jsonPatchDocument = new JsonPatchDocument<AssetEstimateSummary>();
            jsonPatchDocument.Replace(_ => _.TotalServiceCharges, newTotalServiceCharges);

            var serializedDoc = JsonConvert.SerializeObject(jsonPatchDocument);
            var requestContent = new StringContent(serializedDoc, Encoding.UTF8, "application/json-patch+json");
            var response = await _httpClient.PatchAsync(uri, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Unexpected status code from Financial Summary API while sending PATCH request: {response.StatusCode} - {responseBody}");
            }
        }
    }
}
