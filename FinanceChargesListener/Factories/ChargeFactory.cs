using FinanceChargesListener.Domain;
using FinanceChargesListener.Infrastructure.Entities;

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
                ChargeYear = chargeEntity.ChargeYear,
                ChargeGroup = chargeEntity.ChargeGroup,
                ChargeSubGroup = chargeEntity.ChargeSubGroup,
                CreatedAt = chargeEntity.CreatedAt,
                CreatedBy = chargeEntity.CreatedBy,
                LastUpdatedAt = chargeEntity.LastUpdatedAt,
                LastUpdatedBy = chargeEntity.LastUpdatedBy,
                DetailedCharges = chargeEntity.DetailedCharges
            };
        }
    }
}
