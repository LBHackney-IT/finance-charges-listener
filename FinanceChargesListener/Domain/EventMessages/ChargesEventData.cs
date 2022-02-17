using System.Collections.Generic;

namespace FinanceChargesListener.Domain.EventMessages
{
    public class ChargesEventData
    {
        public List<DetailedChargeChange> NewData { get; set; }
    }
}
