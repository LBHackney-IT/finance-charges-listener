using System;
using System.Collections.Generic;

namespace FinanceChargesListener.Domain
{
    public class ChargeMaintenance
    {
        public Guid ChargesId { get; set; }

        public IEnumerable<DetailedCharges> ExistingValue { get; set; }

        public IEnumerable<DetailedCharges> NewValue { get; set; }

        public string Reason { get; set; }

        public DateTime StartDate { get; set; }

        public ChargeMaintenanceStatus Status { get; set; }

    }
}
