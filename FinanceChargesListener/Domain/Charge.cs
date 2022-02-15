using System;
using System.Collections.Generic;

namespace FinanceChargesListener.Domain
{
    public class Charge
    {
        public Guid Id { get; set; }
        public Guid TargetId { get; set; }
        public TargetType TargetType { get; set; }
        public ChargeGroup ChargeGroup { get; set; }
        public ChargeSubGroup? ChargeSubGroup { get; set; }
        public short ChargeYear { get; set; }
        public IEnumerable<DetailedCharges> DetailedCharges { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
