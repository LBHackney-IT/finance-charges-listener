using Amazon.DynamoDBv2.Model;
using Hackney.Shared.Asset.Domain;
using System;

namespace FinanceChargesListener.Infrastructure
{
    public static class QueryResponseExtension
    {
        public static Asset ToAsset(this QueryResponse response)
        {
            throw new NotImplementedException();
        }
    }
}
