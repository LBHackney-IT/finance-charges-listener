using System;
using static FinanceChargesListener.Domain.Enums;

namespace FinanceChargesListener.Domain
{
    public class DetailedCharges
    {
        public string Type { get; set; }

        public string SubType { get; set; }

        public ChargeType ChargeType { get; set; }

        public string ChargeCode { get; set; }

        public string Frequency { get; set; }

        public decimal Amount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
