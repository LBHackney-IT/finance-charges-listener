using FinanceChargesListener.Domain;
using FinanceChargesListener.Infrastructure.Entity;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using System;
using System.Collections.Generic;
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
                DetailedCharges = response.DetailedCharges
            };
        }

        
        //public static Charge ToDomain(this AddChargeRequest chargeRequest)
        //{
        //    if (chargeRequest == null)
        //    {
        //        return null;
        //    }

        //    return new Charge
        //    {
        //        TargetId = chargeRequest.TargetId,
        //        TargetType = chargeRequest.TargetType,
        //        ChargeGroup = chargeRequest.ChargeGroup,
        //        DetailedCharges = chargeRequest.DetailedCharges
        //    };
        //}

        //public static Charge ToDomain(this UpdateChargeRequest chargeRequest)
        //{
        //    if (chargeRequest == null)
        //    {
        //        return null;
        //    }

        //    return new Charge
        //    {
        //        Id = chargeRequest.Id,
        //        TargetId = chargeRequest.TargetId,
        //        TargetType = chargeRequest.TargetType,
        //        ChargeGroup = chargeRequest.ChargeGroup,
        //        DetailedCharges = chargeRequest.DetailedCharges
        //    };
        //}
    }
}
