using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Infrastructure.Entity;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinanceChargesListener.Factories
{
    public static class ChargeFactory
    {
        public static Charge ToDomain(this ChargeDbEntity chargeEntity)
        {
            if (chargeEntity == null)
            {
                return null;
            }

            return new Charge
            {
                Id = chargeEntity.Id,
                TargetId = chargeEntity.TargetId,
                TargetType = chargeEntity.TargetType,
                ChargeGroup = chargeEntity.ChargeGroup,
                ChargeYear = chargeEntity.ChargeYear,
                ChargeSubGroup = chargeEntity.ChargeSubGroup,
                DetailedCharges = chargeEntity.DetailedCharges
            };
        }

        public static ChargeDbEntity ToDatabase(this Charge charge)
        {
            if (charge == null)
            {
                return null;
            }

            return new ChargeDbEntity
            {
                Id = charge.Id,
                TargetId = charge.TargetId,
                TargetType = charge.TargetType,
                ChargeGroup = charge.ChargeGroup,
                ChargeYear = charge.ChargeYear,
                ChargeSubGroup = charge.ChargeSubGroup,
                DetailedCharges = charge.DetailedCharges
            };
        }

        public static ChargeUpdate ToUpdateModel(this Charge response)
        {
            return new ChargeUpdate()
            {
                Id = response.Id,
                ChargeGroup = response.ChargeGroup,
                TargetId = response.TargetId,
                TargetType = response.TargetType,
                ChargeYear = response.ChargeYear,
                ChargeSubGroup = response.ChargeSubGroup,
                DetailedCharges = response.DetailedCharges
            };
        }
        public static Charge ToResponseModel(this ChargeUpdate response)
        {
            return new Charge()
            {
                Id = response.Id,
                ChargeGroup = response.ChargeGroup,
                TargetId = response.TargetId,
                TargetType = response.TargetType,
                ChargeYear = response.ChargeYear,
                ChargeSubGroup = response.ChargeSubGroup,
                DetailedCharges = response.DetailedCharges
            };
        }
        public static Dictionary<string, AttributeValue> ToQueryRequest(this Charge charge)
        {
            return new Dictionary<string, AttributeValue>()
            {
               {"target_id", new AttributeValue {S = charge.TargetId.ToString()}},
                {"id", new AttributeValue {S = charge.Id.ToString()}},
                {"target_type", new AttributeValue {S = charge.TargetType.ToString()}},
                {"charge_group", new AttributeValue {S = charge.ChargeGroup.ToString()}},
                {"charge_sub_group", new AttributeValue {S = charge.ChargeSubGroup.ToString()}},
                {"charge_year", new AttributeValue {N = charge.ChargeYear.ToString()}},
                {
                    "detailed_charges", new AttributeValue
                            {
                                L = charge.DetailedCharges
                                    .Select(p =>
                                        new AttributeValue
                                        {
                                            M = new Dictionary<string, AttributeValue>
                                            {
                                                {"chargeCode", new AttributeValue {S = p.ChargeCode}},
                                                {"frequency", new AttributeValue {S = p.Frequency}},
                                                {"amount", new AttributeValue {N = p.Amount.ToString("F")}},
                                                {"endDate", new AttributeValue {S = p.EndDate.ToString(Constants.UtcDateFormat)}},
                                                {"chargeType", new AttributeValue {S = p.ChargeType.ToString()}},
                                                {"subType", new AttributeValue {S = p.SubType.ToString()}},
                                                {"type", new AttributeValue {S = p.Type.ToString()}},
                                                {"startDate", new AttributeValue {S = p.StartDate.ToString(Constants.UtcDateFormat)}}
                                            }
                                        }
                                    ).ToList()
                            }
                },
                {"created_by", new AttributeValue {S = charge.CreatedBy}},
                {"created_at", new AttributeValue {S = charge.CreatedAt.ToString("F")}}
            };
        }

    }
}
