using System.Collections.Generic;

namespace FinanceChargesListener.Domain
{
    public class AssetList
    {
        public IEnumerable<AssetDetail> AssetDetails { get; set; }
    }
}
