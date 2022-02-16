using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Factories;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Infrastructure;
using FinanceChargesListener.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class ChargesGateway : IChargesGateway
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private readonly IDynamoDBContext _dynamoDbContext;

        public ChargesGateway(IAmazonDynamoDB amazonDynamoDb,
            IDynamoDBContext dynamoDbContext)
        {
            _amazonDynamoDb = amazonDynamoDb;
            _dynamoDbContext = dynamoDbContext;
        }

        public async Task<Charge> GetById(Guid chargeId, Guid assetId)
        {
            var result = await _dynamoDbContext.LoadAsync<ChargeDbEntity>(assetId, chargeId).ConfigureAwait(false);

            return result?.ToDomain();
        }

        public async Task<List<Charge>> GetAllByAssetId(Guid assetId)
        {
            if (assetId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(assetId));
            }

            QueryRequest request = new QueryRequest
            {
                TableName = "Charges",
                KeyConditionExpression = "target_id = :V_target_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":V_target_id",new AttributeValue{S = assetId.ToString()}}
                },
                ScanIndexForward = true
            };

            var response = await _amazonDynamoDb.QueryAsync(request).ConfigureAwait(false);

            return response?.ToCharge();
        }
    }
}
