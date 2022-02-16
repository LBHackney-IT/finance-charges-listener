using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Factories;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Infrastructure;
using FinanceChargesListener.Infrastructure.Entities;
using Hackney.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class ChargesDbGateway : IChargesGateway
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private readonly IDynamoDBContext _dynamoDbContext;
        public const int PerBatchProcessingCount = 25;

        public ChargesDbGateway(IAmazonDynamoDB amazonDynamoDb,
            IDynamoDBContext dynamoDbContext)
        {
            _amazonDynamoDb = amazonDynamoDb;
            _dynamoDbContext = dynamoDbContext;
        }

        [LogCall]
        public async Task<Charge> GetById(Guid chargeId, Guid assetId)
        {
            var result = await _dynamoDbContext.LoadAsync<ChargeDbEntity>(assetId, chargeId).ConfigureAwait(false);

            return result?.ToDomain();
        }

        [LogCall]
        public async Task<bool> SaveBatchAsync(List<Charge> charges)
        {
            var chargesBatch = _dynamoDbContext.CreateBatchWrite<ChargeDbEntity>();

            var items = charges.ToDatabaseList();
            var maxBatchCount = PerBatchProcessingCount;
            if (items.Count > maxBatchCount)
            {
                var loopCount = (items.Count / maxBatchCount) + 1;
                for (var start = 0; start < loopCount; start++)
                {
                    var itemsToWrite = items.Skip(start * maxBatchCount).Take(maxBatchCount);
                    chargesBatch.AddPutItems(itemsToWrite);
                    await chargesBatch.ExecuteAsync().ConfigureAwait(false);
                }
            }
            else
            {
                chargesBatch.AddPutItems(items);
                await chargesBatch.ExecuteAsync().ConfigureAwait(false);
            }

            return true;
        }

        [LogCall]
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
