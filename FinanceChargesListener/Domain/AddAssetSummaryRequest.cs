using System;

namespace FinanceChargesListener.Domain
{
    public class AddAssetSummaryRequest
    {
        public Guid TargetId { get; set; }

        public TargetType TargetType { get; set; }
        public ValuesType ValuesType { get; set; }

        public string AssetName { get; set; }

        public decimal TotalServiceCharges { get; set; }

        public DateTime SubmitDate { get; set; }

        public short SummaryYear { get; set; }

        public int TotalLeaseholders { get; set; }

        public int TotalFreeholders { get; set; }

        public int TotalDwellings { get; set; }

        public int TotalBlocks { get; set; }
    }
}
