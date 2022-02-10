using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Factories;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class ChargesApiGateway : Interfaces.ChargesApiGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly IAmazonDynamoDB _amazonDynamoDb;

        public ChargesApiGateway(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB amazonDynamoDb)
        {
            _dynamoDbContext = dynamoDbContext;
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task AddChargeAsync(Charge charge)
        {
            var chargeDbEntity = charge.ToDatabase();
            chargeDbEntity.CreatedAt = DateTime.UtcNow;
            chargeDbEntity.CreatedBy = "ChargeListener";
            await _dynamoDbContext.SaveAsync(chargeDbEntity).ConfigureAwait(false);
        }

        public async Task<List<Charge>> GetChargeByTargetIdAsync(Guid targetId)
        {
            var request = new QueryRequest
            {
                TableName = "Charges",
                KeyConditionExpression = "target_id = :V_target_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":V_target_id",new AttributeValue{S = targetId.ToString()}}
                },
                ScanIndexForward = true
            };

            var chargesLists = await _amazonDynamoDb.QueryAsync(request).ConfigureAwait(false);

            return chargesLists?.ToChargeDomain();
        }

        public async Task UpdateChargeAsync(Charge charge)
        {
            var chargeDbEntity = charge.ToDatabase();
            chargeDbEntity.LastUpdatedAt = DateTime.UtcNow;
            chargeDbEntity.LastUpdatedBy = "ChargeListener";
            await _dynamoDbContext.SaveAsync(chargeDbEntity).ConfigureAwait(false);
        }
    }
}
