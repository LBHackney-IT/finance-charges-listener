using FinanceChargesListener.Domain;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class AssetSummaryGateway : IAssetSummaryGateway
    {
        private readonly IHousingData<AssetSummary> _housingData;

        public AssetSummaryGateway(IHousingData<AssetSummary> housingData)
        {
            _housingData = housingData;
        }

        public async Task<AssetSummary> UpdateAssetSummaryAsync(Guid assetId, decimal totalServiceCharges)
        {
            if (assetId == Guid.Empty)
                throw new ArgumentException($"{nameof(assetId)} shouldn't be empty.");

            
        }

        public Task<AssetSummary> GetAssetSummaryByAssetIdAsync(Guid assetId)
        {
            if (assetId == Guid.Empty)
                throw new ArgumentException($"{nameof(assetId)} shouldn't be empty.");

            return GetAssetSummaryByAssetIdAsync(assetId);
        }
    }
}
