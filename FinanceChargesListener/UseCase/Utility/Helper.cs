using FinanceChargesListener.Domain;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Tenure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceChargesListener.UseCase.Utility
{
    public static class Helper
    {
        public static double GetScFactor(int numberOfBedrooms)
        {
            var scFactor = numberOfBedrooms switch
            {
                0 => 1.5,
                1 => 3,
                2 => 4,
                3 => 4.5,
                4 => 5,
                5 => 5,
                6 => 5,
                7 => 5,
                _ => 0
            };
            return scFactor;
        }
        public static Dictionary<Guid, double> GetPropertiesWithPercentage(List<Asset> assets)
        {
            var calculatedList = new Dictionary<Guid, double>();

            foreach (var asset in assets)
            {
                if (asset.AssetCharacteristics != null)
                {
                    var scFactor = GetScFactor(asset.AssetCharacteristics.NumberOfBedrooms ?? 0);
                    calculatedList.Add(asset.Id, scFactor);
                }
            }

            var sumOfScFactor = calculatedList.Sum(x => x.Value);
            var perPropVal = 100 / sumOfScFactor;

            foreach (var item in calculatedList.Keys.ToList())
            {
                calculatedList[item] = perPropVal * calculatedList[item];
            }

            return calculatedList;
        }
        public static bool IsChargeApplicableForTenants(string chargeName)
        {
            var result = chargeName switch
            {
                var x when x == HeadOfCharges.BlockCleaning ||
                x == HeadOfCharges.EstateCleaning ||
                x == HeadOfCharges.BuildingInsurance ||
                x == HeadOfCharges.EstateElectricity ||
                x == HeadOfCharges.EstateCCTVMaintenanceAndMonitoring ||
                x == HeadOfCharges.GroundsMaintenance ||
                x == HeadOfCharges.CommunalTVAerial ||
                x == HeadOfCharges.HeatingFuel ||
                x == HeadOfCharges.ConciergeService => true,
                _ => false
            };
            return result;
        }
        public static DateTime GetFirstMondayForApril(int year)
        {
            var dt = new DateTime(year, 4, 1);
            while (dt.DayOfWeek != DayOfWeek.Monday)
            {
                dt = dt.AddDays(1);
            }
            return dt;
        }


        public static List<string> DistinctParentAssetTypes(List<Asset> assets)
        {
            var result = new List<string>();
            assets.ForEach(x =>
            {
                if (x.AssetLocation.ParentAssets.Any())
                {
                    foreach (var assetLocationParentAsset in x.AssetLocation.ParentAssets)
                    {
                        if (!result.Contains(assetLocationParentAsset.Type))
                            result.Add(assetLocationParentAsset.Type);
                    }
                }
            });
            return result;
        }

        public static List<Asset> FilterAssetOnParentId(List<Asset> assets, Guid id)
        {
            var result = new List<Asset>();
            assets.ForEach(x =>
            {
                if (x.AssetLocation.ParentAssets.Any())
                {
                    foreach (var assetLocationParentAsset in x.AssetLocation.ParentAssets)
                    {
                        if (id == assetLocationParentAsset.Id)
                            result.Add(x);
                    }
                }
            });
            return result;
        }
        public static int GetLeaseholdersCount(List<EstimateActualCharge> estimateOrActualCharges)
        {
            if (estimateOrActualCharges == null || !estimateOrActualCharges.Any()) return 0;
            var filteredData = estimateOrActualCharges.Where(
                   x => x.TenureType == TenureTypes.LeaseholdRTB.Description
                || x.TenureType == TenureTypes.PrivateSaleLH.Description
                || x.TenureType == TenureTypes.SharedOwners.Description
                || x.TenureType == TenureTypes.SharedEquity.Description
                || x.TenureType == TenureTypes.ShortLifeLse.Description
                || x.TenureType == TenureTypes.LeaseholdStair.Description
            );
            return filteredData.Count();

        }
        public static int GetFreeholdersCount(List<EstimateActualCharge> estimateOrActualCharges)
        {
            if (estimateOrActualCharges == null || !estimateOrActualCharges.Any()) return 0;
            var filteredData = estimateOrActualCharges.Where(
                   x => x.TenureType == TenureTypes.FreeholdServ.Description
            );
            return filteredData.Count();

        }
        public static List<string> DistinctTenureTypes(List<Asset> assets)
        {
            var result = new List<string>();
            assets.ForEach(x =>
            {
                if (x.Tenure != null && !result.Contains(x.Tenure?.Type))
                {
                    result.Add(x.Tenure.Type);
                }
            });
            return result;
        }

        public static Charge GetChargeModel(Guid assetId, string assetType, decimal chargeAmount, string chargeName,
            string chargeCode, ChargeGroup chargeGroup, ChargeType chargeType)
        {
            var detailedChargesList = new List<DetailedCharges>
            {
                new DetailedCharges
                {
                    Type = "Service",
                    SubType = chargeName,
                    ChargeCode = chargeCode,
                    Amount = chargeAmount,
                    ChargeType = chargeType,
                    Frequency = ChargeFrequency.Monthly.ToString(),
                    StartDate = DateTime.UtcNow,
                    EndDate = GetFirstMondayForApril(DateTime.UtcNow.AddYears(1).Year).AddDays(-1)
                }
            };
            var newCharge = new Charge
            {
                Id = Guid.NewGuid(),
                TargetId = assetId,
                ChargeGroup = chargeGroup,
                TargetType = (TargetType) Enum.Parse(typeof(TargetType), assetType),
                DetailedCharges = detailedChargesList.AsEnumerable(),
            };
            return newCharge;

        }

        public static DetailedCharges GetChargeDetailModel(decimal chargeAmount,
            string chargeName, string chargeCode, ChargeGroup chargeGroup, ChargeType chargeType)
        {
            return new DetailedCharges
            {
                Type = "Service",
                SubType = chargeName,
                ChargeCode = chargeCode,
                Amount = chargeAmount,
                ChargeType = chargeType,
                Frequency = chargeGroup == ChargeGroup.Tenants
                            ? ChargeFrequency.Weekly.ToString()
                            : ChargeFrequency.Monthly.ToString(),
                StartDate = DateTime.UtcNow,
                EndDate = GetFirstMondayForApril(DateTime.UtcNow.AddYears(1).Year).AddDays(-1)
            };
        }
        public static List<Asset> GetAssetsWithEstate(List<Asset> assets)
        {
            var result = new List<Asset>();
            assets.ForEach(x =>
            {
                if (x.AssetLocation.ParentAssets.Any())
                {
                    foreach (var assetLocationParentAsset in x.AssetLocation.ParentAssets)
                    {
                        if (assetLocationParentAsset.Type == "Estate")
                            result.Add(x);
                    }
                }
            });
            return result;
        }
        public static List<Asset> GetAssetsWithBlock(List<Asset> assets)
        {
            var result = new List<Asset>();
            assets.ForEach(x =>
            {
                if (x.AssetLocation.ParentAssets.Any())
                {
                    foreach (var assetLocationParentAsset in x.AssetLocation.ParentAssets)
                    {
                        if (assetLocationParentAsset.Type == "Block"
                        || assetLocationParentAsset.Type == "High Rise Block (6 storeys or more)"
                        || assetLocationParentAsset.Type == "Low Rise Block (2 storeys or less)"
                        || assetLocationParentAsset.Type == "Medium Rise Block (3-5 storeys)"
                        || assetLocationParentAsset.Type == "Walk-Up Block")
                            result.Add(x);
                    }
                }
            });
            return result;
        }
    }
}
