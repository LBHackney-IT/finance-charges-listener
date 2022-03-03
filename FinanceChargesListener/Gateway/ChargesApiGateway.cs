using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Factories;
using FinanceChargesListener.Infrastructure;
using FinanceChargesListener.Infrastructure.Entities;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinanceChargesListener.Gateway.Common;

namespace FinanceChargesListener.Gateway
{
    public class ChargesApiGateway : Interfaces.IChargesApiGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private readonly ILogger<ChargesApiGateway> _logger;

        public const int PerBatchProcessingCount = 25;

        public ChargesApiGateway(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB amazonDynamoDb,
            ILogger<ChargesApiGateway> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _amazonDynamoDb = amazonDynamoDb;
            _logger = logger;
        }

        [LogCall]
        public async Task AddChargeAsync(Charge charge)
        {
            var chargeDbEntity = charge.ToDatabase();
            chargeDbEntity.CreatedAt = DateTime.UtcNow;
            chargeDbEntity.CreatedBy = "ChargeListener";
            await _dynamoDbContext.SaveAsync(chargeDbEntity).ConfigureAwait(false);
        }

        [LogCall]
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

            return chargesLists?.ToCharge();
        }

        [LogCall]
        public async Task UpdateChargeAsync(Charge charge)
        {
            var chargeDbEntity = charge.ToDatabase();
            chargeDbEntity.LastUpdatedAt = DateTime.UtcNow;
            chargeDbEntity.LastUpdatedBy = "ChargeListener";
            await _dynamoDbContext.SaveAsync(chargeDbEntity).ConfigureAwait(false);
        }

        [LogCall]
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
                int loopCount;
                if (charges.Count % maxBatchCount == 0)
                    loopCount = charges.Count / maxBatchCount;
                else
                    loopCount = (charges.Count / maxBatchCount) + 1;

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

        public async Task DeleteBatchAsync(List<ChargeKeys> chargeIds, int batchCapacity)
        {
            if (batchCapacity <= 0)
            {
                return;
            }
            int loopCount;
            if (chargeIds.Count % batchCapacity == 0)
                loopCount = chargeIds.Count / batchCapacity;
            else
                loopCount = (chargeIds.Count / batchCapacity) + 1;

            for (int i = 0; i < loopCount; i++)
            {
                await DeleteBatchAsync(chargeIds.Skip(i * batchCapacity).Take(batchCapacity).ToList())
                    .ConfigureAwait(false);
            }
        }

        private async Task DeleteBatchAsync(List<ChargeKeys> chargeIds)
        {
            var request = new BatchWriteItemRequest
            {
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
                RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    {
                        Constants.ChargeTableName,
                        chargeIds.ToWriteRequests().ToList()
                    }
                }
            };

            BatchWriteItemResponse response;
            do
            {
                LoggingHandler.LogInfo("*** Deleting Charges Starting");
                response = await _amazonDynamoDb.BatchWriteItemAsync(request).ConfigureAwait(false);

                request.RequestItems = response.UnprocessedItems;
            }
            while (response.UnprocessedItems.Count > 0);
            LoggingHandler.LogInfo("*** Deleting Charges Completed");
        }

        public async Task<List<ChargeKeys>> GetChargesByYearGroupSubGroup(short chargeYear, ChargeGroup chargeGroup, ChargeSubGroup? chargeSubGroup)
        {
            int totalSegments = 5;
            var finalResult = new List<Charge>();
            LoggingHandler.LogInfo($"*** Creating {totalSegments} Parallel Scan Tasks to scan {Constants.ChargeTableName}");
            Task[] tasks = new Task[totalSegments];
            for (int segment = 0; segment < totalSegments; segment++)
            {
                int tmpSegment = segment;
                Task task = await Task.Factory.StartNew(async () =>
                {
                    var scanSegmentResult = await ScanSegment(totalSegments, tmpSegment, chargeYear, chargeGroup, chargeSubGroup).ConfigureAwait(false);

                    finalResult.AddRange(scanSegmentResult);
                }).ConfigureAwait(false);

                tasks[segment] = task;
            }

            LoggingHandler.LogInfo("All scan tasks are created, waiting for them to complete.");
            Task.WaitAll(tasks);

            LoggingHandler.LogInfo("All scan tasks are completed.");
            LoggingHandler.LogInfo($"*** Completed Count:  {finalResult.Count} ");


            LoggingHandler.LogInfo("Scan completed");

           // return finalResult;

            return finalResult.Select(i => i.GetChargeKeys()).ToList();
        }
        private async Task<List<Charge>> ScanSegment(int totalSegments, int segment, short chargeYear, ChargeGroup chargeGroup, ChargeSubGroup? chargeSubGroup)
        {
            var resultList = new List<Charge>();

            LoggingHandler.LogInfo($"*** Starting to Scan Segment {segment} of {Constants.ChargeTableName} out of {totalSegments} total segments ***");
            Dictionary<string, AttributeValue> lastEvaluatedKey = null;
            int totalScannedItemCount = 0;
            int totalScanRequestCount = 0;
            do
            {
                var request = new ScanRequest
                {
                    TableName = Constants.ChargeTableName,
                    Limit = 1200,
                    ExclusiveStartKey = lastEvaluatedKey,
                    Segment = segment,
                    TotalSegments = totalSegments
                };

                var response = await _amazonDynamoDb.ScanAsync(request).ConfigureAwait(false);
                lastEvaluatedKey = response.LastEvaluatedKey;
                totalScanRequestCount++;
                totalScannedItemCount += response.ScannedCount;

                var scannedResult = response.ToChargeDomain();
                LoggingHandler.LogInfo($"*** Completed Scan Count : {scannedResult.Count} ");
                var filteredList = scannedResult?.Where(x => x.ChargeYear == chargeYear
                                                             && x.ChargeGroup == chargeGroup
                                                             && x.ChargeSubGroup == chargeSubGroup);

                resultList.AddRange(filteredList);
                LoggingHandler.LogInfo($"*** Completed Filtered Count:  {resultList.Count} ");
                Thread.Sleep(2000);
            } while (lastEvaluatedKey.Count != 0);

            LoggingHandler.LogInfo($"*** Completed Scan Segment {segment} of {Constants.ChargeTableName}. TotalScanRequestCount: {totalScanRequestCount}, TotalScannedItemCount: {totalScannedItemCount} ***");
            return resultList;
        }
    }
}
