using System;
using System.Collections.Generic;
using System.Text;
using FinanceChargesListener.Domain;

namespace FinanceChargesListener.Boundary
{
    public class PropertyChargesMessageSqs
    {
        public short ChargeYear { get; set; }

        public ChargeGroup ChargeGroup { get; set; }

        public ChargeSubGroup ChargeSubGroup { get; set; }
    }
}
