using Amazon.DynamoDBv2.Model;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Boundary
{
    public class AssetPaginationResponse
    {
        public Dictionary<string, AttributeValue> LastKey { get; set; }
        public List<Asset> Assets { get; set; }
    }
}
