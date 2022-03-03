using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FinanceChargesListener.Infrastructure
{
    public static class QueryResponseExtension
    {
        public static List<Charge> ToCharge(this QueryResponse response)
        {
            var chargesList = new List<Charge>();
            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                var detailCharges = new List<DetailedCharges>();
                var innerItem = item["detailed_charges"].L;
                foreach (var detail in innerItem)
                {

                    detailCharges.Add(new DetailedCharges
                    {
                        Amount = Convert.ToDecimal(detail.M["amount"].N, CultureInfo.InvariantCulture),
                        ChargeCode = detail.M["chargeCode"].S,
                        ChargeType = Enum.Parse<ChargeType>(detail.M["chargeType"].S),
                        Type = detail.M["type"].S,
                        SubType = detail.M["subType"].S,
                        Frequency = detail.M["frequency"].S,
                        StartDate = DateTime.Parse(detail.M["startDate"].S),
                        EndDate = DateTime.Parse(detail.M["endDate"].S)
                    });
                }
                var chargeYear = !string.IsNullOrEmpty(item["charge_year"].N)
                        ? Convert.ToInt16(item["charge_year"].N) : 0;
                chargesList.Add(new Charge
                {
                    Id = Guid.Parse(item["id"].S),
                    TargetId = Guid.Parse(item["target_id"].S),
                    ChargeGroup = Enum.Parse<ChargeGroup>(item["charge_group"].S),
                    ChargeSubGroup = item.ContainsKey("charge_sub_group") ? Enum.Parse<ChargeSubGroup>(item["charge_sub_group"].S) : (ChargeSubGroup?) null,
                    TargetType = Enum.Parse<TargetType>(item["target_type"].S),
                    ChargeYear = Convert.ToInt16(chargeYear),
                    DetailedCharges = detailCharges
                });
            }

            return chargesList;
        }

        public static IEnumerable<Asset> ToAssets(this ScanResponse response)
        {
            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                yield return new Asset
                {
                    Id = Guid.Parse(item["id"].S),
                    AssetId = item.ContainsKey("assetId") ? (item["assetId"].NULL ? null : item["assetId"].S) : null,
                    AssetType = Enum.Parse<AssetType>(item["assetType"].S),
                };
            }
        }
        public static AssetKeys GetAssetKeys(this Dictionary<string, AttributeValue> scanResponseItem)
           => new AssetKeys(Guid.Parse(scanResponseItem["id"].S), scanResponseItem["assetId"].S);

        public static ChargeKeys GetChargeKeys(this Charge charge)
           => new ChargeKeys(charge.Id, charge.TargetId);

        /// <summary>
        /// Returns a list of WriteRequest objects with [id] attributes only
        /// </summary>
        /// <param name="chargeIds"></param>
        /// <returns></returns>
        public static IEnumerable<WriteRequest> ToWriteRequests(this IEnumerable<ChargeKeys> chargeIds)
            => chargeIds.Select(c => new WriteRequest
            {
                DeleteRequest = new DeleteRequest
                {
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "id", new AttributeValue { S = c.Id.ToString() } },
                        { "target_id", new AttributeValue { S = c.TargetId.ToString() } }
                    }
                }
            });

        public static List<Charge> ToChargeDomain(this ScanResponse response)
        {
            var chargesList = new List<Charge>();
            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                var detailCharges = new List<DetailedCharges>();
                var innerItem = item.ContainsKey("detailed_charges") ? item["detailed_charges"].L : null;

                if (innerItem != null)
                {
                    foreach (var detail in innerItem)
                    {
                        detailCharges.Add(new DetailedCharges
                        {
                            Amount = Convert.ToDecimal(detail.M["amount"].N, CultureInfo.InvariantCulture),
                            ChargeCode = detail.M["chargeCode"].S,
                            ChargeType = Enum.Parse<ChargeType>(detail.M["chargeType"].S),
                            Type = detail.M["type"].S,
                            SubType = detail.M["subType"].S,
                            Frequency = detail.M["frequency"].S,
                            StartDate = DateTime.Parse(detail.M["startDate"].S),
                            EndDate = DateTime.Parse(detail.M["endDate"].S)
                        });
                    }
                }

                chargesList.Add(new Charge
                {
                    Id = Guid.Parse(item["id"].S),
                    TargetId = Guid.Parse(item["target_id"].S),
                    ChargeGroup = Enum.Parse<ChargeGroup>(item["charge_group"].S),
                    ChargeSubGroup = item.ContainsKey("charge_sub_group") ? Enum.Parse<ChargeSubGroup>(item["charge_sub_group"].S) : ((ChargeSubGroup?) null),
                    TargetType = Enum.Parse<TargetType>(item["target_type"].S),
                    ChargeYear = item.ContainsKey("charge_year") ? Convert.ToInt16(item["charge_year"].N) : Convert.ToInt16(0),
                    DetailedCharges = detailCharges
                });
            }

            return chargesList;
        }
    }
}
