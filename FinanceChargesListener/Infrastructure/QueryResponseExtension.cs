using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;

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
    }
}
