using System;
using System.Collections.Generic;
using static FinanceChargesListener.Domain.Enums;

namespace FinanceChargesListener.Domain
{
    public class AddCharge
    {
        public Guid TargetId { get; set; }
        public TargetType TargetType { get; set; }
        public ChargeGroup ChargeGroup { get; set; }
        public IEnumerable<DetailedCharges> DetailedCharges { get; set; }
    }
}
