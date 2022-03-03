namespace FinanceChargesListener.Domain.EventMessages
{
    public class DetailedChargeChange
    {
        public string SubType { get; set; }
        public ChargeType ChargeType { get; set; }
        public decimal DifferenceAmount { get; set; }
    }
}
