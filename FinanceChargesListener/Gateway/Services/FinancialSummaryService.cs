using FinanceChargesListener.Domain;
using FinanceChargesListener.Gateway.Extensions;
using FinanceChargesListener.Gateway.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Services
{
    public class FinancialSummaryService : IFinancialSummaryService
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _contextAccessor;

        public FinancialSummaryService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<bool> AddEstimateSummary(AddAssetSummaryRequest addAssetSummaryRequest)
        {
            var response = await _client.PostAsJsonAsyncType(new Uri("api/v1/asset-summary", UriKind.Relative), addAssetSummaryRequest)
                .ConfigureAwait(true);
            if (response)
                return true;
            else
                return false;
        }

        public async Task<bool> AddEstimateActualSummaryBatch(IEnumerable<AddAssetSummaryRequest> addAssetSummariesRequest)
        {
            var response = await _client.PostAsJsonAsyncType(new Uri("api/v1/asset-summary/process-batch", UriKind.Relative), addAssetSummariesRequest)
                .ConfigureAwait(true);
            if (response)
                return true;
            else
                return false;
        }
    }
}
