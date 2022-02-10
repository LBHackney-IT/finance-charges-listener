using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Factories
{
    public class EsFactory
    {
        public static QueryableAsset ToQueryableAsset(Asset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            if (asset.Tenure == null)
                throw new Exception("There is no tenure provided for this asset.");

            return new QueryableAsset
            {
                Id = asset.Id.ToString(),
                AssetId = asset.AssetId,
                AssetType = asset.AssetType.ToString(),
                IsAssetCautionaryAlerted = false,
                AssetAddress = new QueryableAssetAddress
                {
                    AddressLine1 = asset.AssetAddress.AddressLine1,
                    AddressLine2 = asset.AssetAddress.AddressLine2,
                    AddressLine3 = asset.AssetAddress.AddressLine3,
                    AddressLine4 = asset.AssetAddress.AddressLine4,
                    PostCode = asset.AssetAddress.PostCode,
                    PostPreamble = asset.AssetAddress.PostPreamble,
                    Uprn = asset.AssetAddress.Uprn
                },
               Tenure = new QueryableAssetTenure
               {
                   EndOfTenureDate = asset.Tenure.EndOfTenureDate.ToString(),
                   Id = asset.Tenure.Id,
                   PaymentReference = asset.Tenure.PaymentReference,
                   StartOfTenureDate = asset.Tenure.StartOfTenureDate.ToString(),
                   Type = asset.Tenure.Type

               }

            };
        }
    }
}
