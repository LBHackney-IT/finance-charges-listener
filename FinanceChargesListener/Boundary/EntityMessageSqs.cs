using System;

namespace FinanceChargesListener.Boundary
{
    public class EntityMessageSqs
    {
        public Guid Id { get; set; }
        public Guid ChargesListId { get; set; }
        public string ChargeName { get; set; }
        public string ChargeCode { get; set; }
        public decimal TotalEstimateAmount { get; set; }
    }
}
