using System.Collections.Generic;
using FinanceChargesListener.Domain;

namespace FinanceChargesListener.Boundary
{
    public class EstateChargeMessageSqs
    {
        public Enums.ChargeGroup ChargeGroup { get; set; }
        public Enums.ChargeType ChargeType { get; set; }
        public IList<ChargeItemSqs> ChargeItems { get; set; }
    }
}
