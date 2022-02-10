using System;
using System.Collections.Generic;
using static FinanceChargesListener.Domain.Enums;

namespace FinanceChargesListener.Domain
{
    public class Charge
    {
        public Guid Id { get; set; }
        public Guid TargetId { get; set; }
        public TargetType TargetType { get; set; }
        public ChargeGroup ChargeGroup { get; set; }
        public short ChargeYear { get; set; }
        public IEnumerable<DetailedCharges> DetailedCharges { get; set; }
    }
}
