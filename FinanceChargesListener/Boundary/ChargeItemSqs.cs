namespace FinanceChargesListener.Boundary
{
    public class ChargeItemSqs
    {
        public string  ChargeName { get; set; }
        public string ChargeCode { get; set; }
        public decimal PerPropertyCharge { get; set; }
        public bool IsChargeApplicable { get; set; }
    }
}
