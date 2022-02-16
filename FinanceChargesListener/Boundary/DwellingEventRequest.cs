using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;

namespace FinanceChargesListener.Boundary
{
    public class DwellingEventRequest
    {
        public Guid AssetId { get; set; }

        public Guid ChargeId { get; set; }

        public string SubType { get; set; }

        public ChargeType ChargeType { get; set; }

        public IEnumerable<DetailedCharges> Details { get; set; }
    }
}
