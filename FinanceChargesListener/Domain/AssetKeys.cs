using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Domain
{
    public class AssetKeys
    {
        public Guid Id { get; }
        public string AssetId { get; }

        public AssetKeys(Guid id, string assetId)
        {
            Id = id;
            AssetId = assetId;
        }
    }
}
