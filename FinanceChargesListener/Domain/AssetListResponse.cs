using System.Collections.Generic;

namespace FinanceChargesListener.Domain
{
    public class AssetListResponse
    {
        public Results Results { get; set; }
        public long Total { get; set; }
        public string LastHitId { get; set; }
    }
}
