using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Factories;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class ChargesApiGateway : Interfaces.ChargesApiGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private readonly ILogger<ChargesApiGateway> _logger;

        public ChargesApiGateway(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB amazonDynamoDb,
            ILogger<ChargesApiGateway> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _amazonDynamoDb = amazonDynamoDb;
            _logger = logger;
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

        public async Task<bool> AddTransactionBatchAsync(List<Charge> charges)
        {
            bool result = false;

            List<TransactWriteItem> actions = new List<TransactWriteItem>();
            foreach (Charge charge in charges)
            {
                Dictionary<string, AttributeValue> columns = new Dictionary<string, AttributeValue>();
                columns = charge.ToQueryRequest();

                actions.Add(new TransactWriteItem
                {
                    Put = new Put()
                    {
                        TableName = Constants.ChargeTableName,
                        Item = columns,
                        ReturnValuesOnConditionCheckFailure = ReturnValuesOnConditionCheckFailure.ALL_OLD,
                        ConditionExpression = Constants.AttributeNotExistId
                    }
                });
            }

            TransactWriteItemsRequest placeOrderCharges = new TransactWriteItemsRequest
            {
                TransactItems = actions,
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
            };

            try
            {
                await _amazonDynamoDb.TransactWriteItemsAsync(placeOrderCharges).ConfigureAwait(false);
                result = true;
            }
            catch (ResourceNotFoundException rnf)
            {
                _logger.LogDebug($"One of the table involved in the transaction is not found: {rnf.Message}");
            }
            catch (InternalServerErrorException ise)
            {
                _logger.LogDebug($"Internal Server Error: {ise.Message}");
            }
            catch (TransactionCanceledException tce)
            {
                _logger.LogDebug($"Transaction Canceled: {tce.Message}");
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Transaction Canceled: {e.Message}");
                throw new Exception(e.Message);
            }
            return result;
        }
    }
}
