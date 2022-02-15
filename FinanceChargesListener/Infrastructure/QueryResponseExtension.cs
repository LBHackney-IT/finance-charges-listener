using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;
using static FinanceChargesListener.Domain.Enums;

namespace FinanceChargesListener.Infrastructure
{
    public static class QueryResponseExtension
    {
        public static List<Charge> ToChargeDomain(this QueryResponse response)
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
                        Amount = Convert.ToDecimal(detail.M["amount"].N),
                        ChargeCode = detail.M["chargeCode"].S,
                        ChargeType = Enum.Parse<ChargeType>(detail.M["chargeType"].S),
                        Type = detail.M["type"].S,
                        SubType = detail.M["subType"].S,
                        Frequency = detail.M["frequency"].S,
                        StartDate = DateTime.Parse(detail.M["startDate"].S),
                        EndDate = DateTime.Parse(detail.M["endDate"].S)
                    });
                }

                chargesList.Add(new Charge
                {
                    Id = Guid.Parse(item["id"].S),
                    TargetId = Guid.Parse(item["target_id"].S),
                    ChargeGroup = Enum.Parse<ChargeGroup>(item["charge_group"].S),
                    TargetType = Enum.Parse<TargetType>(item["target_type"].S),
                    DetailedCharges = detailCharges
                });
            }

            return chargesList;
        }
    }
}
