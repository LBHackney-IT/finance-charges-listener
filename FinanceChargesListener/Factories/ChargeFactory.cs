using FinanceChargesListener.Domain;
using FinanceChargesListener.Infrastructure.Entities;
using System.Collections.Generic;
using System.Linq;

namespace FinanceChargesListener.Factories
{
    public static class ChargeFactory
    {
        public static Charge ToDomain(this ChargeDbEntity chargeEntity) => chargeEntity == null ? null : new Charge
        {
            Id = chargeEntity.Id,
            TargetId = chargeEntity.TargetId,
            TargetType = chargeEntity.TargetType,
            ChargeYear = chargeEntity.ChargeYear,
            ChargeGroup = chargeEntity.ChargeGroup,
            ChargeSubGroup = chargeEntity.ChargeSubGroup,
            CreatedAt = chargeEntity.CreatedAt,
            CreatedBy = chargeEntity.CreatedBy,
            LastUpdatedAt = chargeEntity.LastUpdatedAt,
            LastUpdatedBy = chargeEntity.LastUpdatedBy,
            DetailedCharges = chargeEntity.DetailedCharges
        };

        public static List<ChargeDbEntity> ToDatabaseList(this List<Charge> charges)
        {
            return charges.Select(item => item.ToDatabase()).ToList();
        }

        public static ChargeDbEntity ToDatabase(this Charge charge) => charge == null ? null : new ChargeDbEntity
        {
            Id = charge.Id,
            TargetId = charge.TargetId,
            TargetType = charge.TargetType,
            ChargeGroup = charge.ChargeGroup,
            ChargeSubGroup = charge.ChargeSubGroup,
            ChargeYear = charge.ChargeYear,
            CreatedAt = charge.CreatedAt,
            CreatedBy = charge.CreatedBy,
            LastUpdatedBy = charge.LastUpdatedBy,
            LastUpdatedAt = charge.LastUpdatedAt,
            DetailedCharges = charge.DetailedCharges
        };
    }
}
