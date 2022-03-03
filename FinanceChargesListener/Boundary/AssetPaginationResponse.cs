using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Boundary
{
    public class AssetPaginationResponse
    {
        public List<AssetKeys> Assets { get; set; }
    }
}
