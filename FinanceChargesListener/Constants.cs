using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener
{
    public class Constants
    {
        public const string ChargeTableName = "Charges";
        public const string AttributeNotExistId = "attribute_not_exists(id)";
        public const string HackneyRootAssetId = "656feda1-896f-b136-da84-163ee4f1be6c";
        public const string RootAsset = "RootAsset";
        public const int PageSize = 8000;
        public const int Page = 1;
        public const int PerBatchProcessingCount = 25;
        public const string UtcDateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
    }
}
