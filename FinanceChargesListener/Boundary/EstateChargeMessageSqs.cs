using System.Collections.Generic;
using FinanceChargesListener.Domain;

namespace FinanceChargesListener.Boundary
{
    public class EstateChargeMessageSqs
    {
        public ChargeGroup ChargeGroup { get; set; }
        public ChargeType ChargeType { get; set; }
        public IList<ChargeItemSqs> ChargeItems { get; set; }
    }
}
