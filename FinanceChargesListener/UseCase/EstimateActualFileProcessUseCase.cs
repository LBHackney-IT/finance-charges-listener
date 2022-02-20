using Amazon.DynamoDBv2.Model;
using Amazon.S3.Model;
using ExcelDataReader;
using FinanceChargesListener.Boundary;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Factories;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Gateway.Services.Interfaces;
using FinanceChargesListener.Infrastructure.Interfaces;
using FinanceChargesListener.UseCase.Interfaces;
using FinanceChargesListener.UseCase.Utility;
using Hackney.Shared.Asset.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase
{
    public class EstimateActualFileProcessUseCase : IEstimateActualFileProcessUseCase
    {
        private readonly IAwsS3FileService _awsS3FileService;
        private readonly ISnsGateway _snsGateway;
        private readonly HousingSearchService _housingSearchService;
        private readonly IAssetInformationApiGateway _assetInformationApiGateway;
        private readonly AssetGateway _assetGateway;
        private readonly IChargesApiGateway _chargesApiGateway;
        private readonly IFinancialSummaryService _financialSummaryService;
        private readonly ILogger<EstimateActualFileProcessUseCase> _logger;

        private static List<Asset> _blockFullList = new List<Asset>();
        private static List<Asset> _estateFullList = new List<Asset>();
        private static List<Charge> _propertyCharges = new List<Charge>();

        public EstimateActualFileProcessUseCase(
            IAwsS3FileService awsS3FileService,
            ISnsGateway snsGateway,
            HousingSearchService housingSearchService,
            IAssetInformationApiGateway assetInformationApiGateway,
            AssetGateway assetGateway,
            IChargesApiGateway chargesApiGateway,
            IFinancialSummaryService financialSummaryService,
            ILogger<EstimateActualFileProcessUseCase> logger)
        {
            _awsS3FileService = awsS3FileService;
            _snsGateway = snsGateway;
            _housingSearchService = housingSearchService;
            _assetInformationApiGateway = assetInformationApiGateway;
            _assetGateway = assetGateway;
            _chargesApiGateway = chargesApiGateway;
            _financialSummaryService = financialSummaryService;
            _logger = logger;
        }
        public async Task ProcessMessageAsync(EntityEventSns message, JsonSerializerOptions jsonSerializerOptions)
        {
            var bucketName = Environment.GetEnvironmentVariable("CHARGES_BUCKET_NAME");
            short chargeYear = 0;
            var chargeSubGroup = string.Empty;

            if (message?.EventData?.NewData != null)
            {
                var fileData = JsonSerializer.Deserialize<EntityFileMessageSqs>(message?.EventData?.NewData?.ToString() ?? string.Empty, jsonSerializerOptions);
                var s3file = await _awsS3FileService.GetFile(bucketName, fileData.RelativePath).ConfigureAwait(false);
                var recordsCount = 0;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var excelData = new List<EstimateActualCharge>();
                // Read Excel
                using (var stream = new MemoryStream())
                {
                    s3file.CopyTo(stream);
                    stream.Position = 1;

                    // Excel Read Process
                    using var reader = ExcelReaderFactory.CreateReader(stream);

                    while (reader.Read())
                    {
                        if (recordsCount == 0)
                        {
                            chargeYear = Convert.ToInt16($"20{reader.GetValue(19).ToString().Substring(0, 2)}");
                            chargeSubGroup = reader.GetValue(19).ToString().Substring(0, 3).EndsWith("E")
                                       ? Constants.EstimateTypeFile
                                       : Constants.ActualTypeFile;

                            _logger.LogDebug($"Extracted the ChargeYear for Estimates Upload as {chargeYear}");
                        }
                        else
                        {
                            try
                            {
                                excelData.Add(new EstimateActualCharge
                                {
                                    PropertyReferenceNumber = reader.GetValue(1).ToString(),
                                    AssetAddress = reader.GetValue(2).ToString(),
                                    TenureType = reader.GetValue(3).ToString(),
                                    BlockId = reader.GetValue(4).ToString(),
                                    BlockAddress = reader.GetValue(5).ToString(),
                                    EstateId = reader.GetValue(6).ToString(),
                                    EstateAddress = reader.GetValue(7).ToString(),
                                    TotalCharge = GetChargeAmount(reader.GetValue(18)),
                                    BlockCCTVMaintenanceAndMonitoring = GetChargeAmount(reader.GetValue(19)),
                                    BlockCleaning = GetChargeAmount(reader.GetValue(20)),
                                    BlockElectricity = GetChargeAmount(reader.GetValue(21)),
                                    BlockRepairs = GetChargeAmount(reader.GetValue(22)),
                                    BuildingInsurancePremium = GetChargeAmount(reader.GetValue(23)),
                                    DoorEntry = GetChargeAmount(reader.GetValue(24)),
                                    CommunalTVAerialMaintenance = GetChargeAmount(reader.GetValue(25)),
                                    ConciergeService = GetChargeAmount(reader.GetValue(26)),
                                    EstateCCTVMaintenanceAndMonitoring = GetChargeAmount(reader.GetValue(27)),
                                    EstateCleaning = GetChargeAmount(reader.GetValue(28)),
                                    EstateElectricity = GetChargeAmount(reader.GetValue(29)),
                                    EstateRepairs = GetChargeAmount(reader.GetValue(30)),
                                    EstateRoadsFootpathsAndDrainage = GetChargeAmount(reader.GetValue(31)),
                                    GroundRent = GetChargeAmount(reader.GetValue(32)),
                                    GroundsMaintenance = GetChargeAmount(reader.GetValue(33)),
                                    HeatingOrHotWaterEnergy = GetChargeAmount(reader.GetValue(34)),
                                    HeatingOrHotWaterMaintenance = GetChargeAmount(reader.GetValue(35)),
                                    HeatingStandingCharge = GetChargeAmount(reader.GetValue(36)),
                                    LiftMaintenance = GetChargeAmount(reader.GetValue(37)),
                                    ManagementCharge = GetChargeAmount(reader.GetValue(38)),
                                    ReserveFund = GetChargeAmount(reader.GetValue(39))
                                });
                            }
                            catch (Exception e)
                            {
                                //_logger.LogDebug($"Exception occurred while reading the Estimates Excel Sheet: {e.Message}");
                                throw new Exception(e.Message);
                            }
                        }
                        recordsCount++;
                    }
                }


                // Extract Blocks List and Estate List by Scanning Assets
                //if (fileData.StepNumber == 1)
                //{
                //    var blocksScanList = await _assetGateway.GetAllByAssetType(AssetType.Block.ToString()).ConfigureAwait(false);
                //    if (blocksScanList != null && blocksScanList.Assets != null && blocksScanList.Assets.Any())
                //    {
                //        _blockFullList.AddRange(blocksScanList.Assets);
                //    }
                //    _logger.LogDebug($"Extracted the Block Assets Total {_blockFullList.Count}");

                //    var estateScanList = await _assetGateway.GetAllByAssetType(AssetType.Estate.ToString()).ConfigureAwait(false);
                //    if (estateScanList != null && estateScanList.Assets != null && estateScanList.Assets.Any())
                //    {
                //        _estateFullList.AddRange(estateScanList.Assets);
                //    }
                //    _logger.LogDebug($"Extracted the Estate Assets Total {_estateFullList.Count}");

                //    await PushMessageToSNS(fileData).ConfigureAwait(false);
                //}

                // Read Excel ,
                // Get All Dwelling Asset,
                // Transform Asset Id,
                // Form Property Charges 
                if (fileData.StepNumber == 1)
                {
                    _logger.LogDebug($"Step {fileData.StepNumber}");
                    if (excelData != null)
                    {
                        var estimatesActual = excelData;

                        // Load Assets, Blocks , EStates 
                        _logger.LogDebug($"Starting fetching assets list from Housing Search API Asset Search Endpoint");
                        var assetsList = await GetAssetsList(AssetType.Dwelling.ToString()).ConfigureAwait(false);
                        var dwellingsListResult = assetsList.Item1;

                        var blockList = await GetAssetsList(AssetType.Block.ToString()).ConfigureAwait(false);
                        _blockFullList = blockList.Item1;

                        var estateList = await GetAssetsList(AssetType.Estate.ToString()).ConfigureAwait(false);
                        _estateFullList = estateList.Item1;

                        _logger.LogDebug($"Assets List fetching completed and total assets fetched : {assetsList.Item1.Count}");



                        // Dwelling Charges Transformation
                        _logger.LogDebug($"Starting UH numerical Asset Id transformation with Guid Asset Id");
                        estimatesActual.ForEach(item =>
                        {
                            var data = dwellingsListResult.FirstOrDefault(x => x.AssetId == item.PropertyReferenceNumber);
                            // TBC
                            if (data == null)
                            {
                                _logger.LogDebug($"Could not find associated Guid Asset Id for UH Asset Id : {item.PropertyReferenceNumber}");
                                item.AssetId = Guid.NewGuid();
                                _logger.LogDebug($"Created Asset Guid for UH Asset Id {item.PropertyReferenceNumber}: {item.AssetId}");
                            }
                            else
                                item.AssetId = data.Id;
                        });

                        // Property Charges
                        _logger.LogDebug($"Starting Charges formation Process");
                        //var propertyCharges = new List<Charge>();
                        var createdBy = Constants.ChargesListenerUserName;
                        foreach (var item in estimatesActual)
                        {
                            _propertyCharges.Add(ChargeHelper.GetChargeModel(AssetType.Dwelling.ToString(),
                                ChargeGroup.Leaseholders, chargeSubGroup, createdBy, chargeYear, item));
                        }
                    }
                    await PushMessageToSNS(fileData).ConfigureAwait(false);
                }

                // Get Excel Data
                // Group by Block Id and Estate Id
                // Create Block Charges List
                // Create Estate Charges List
                // Create Hackney Total Charge
                // Write All Property Charges
                if (fileData.StepNumber == 2)
                {
                    _logger.LogDebug($"Step {fileData.StepNumber}");
                    if (excelData != null)
                    {
                        if (!_propertyCharges.Any())
                            _logger.LogDebug($"Property Charges is null");

                        _logger.LogDebug($"Property Charge Write Starting");
                        // Charges Load
                        var data = _propertyCharges.OrderBy(x => x.TargetId).Skip(fileData.WriteIndex * 500).Take(500).ToList();
                        if (data.Any())
                        {
                            var writeResult = await _chargesApiGateway.SaveBatchAsync(data.ToList()).ConfigureAwait(false);

                            _logger.LogDebug($"Property Charge Write Completed");
                            if (writeResult)
                                await PushMessageToSNS(fileData, ++fileData.WriteIndex, false).ConfigureAwait(false);
                        }
                        else
                            await PushMessageToSNS(fileData).ConfigureAwait(false);
                    }
                }

                // Write All Block Charges
                // Write All Estate Charges
                // Write Hackney Total Charge
                if (fileData.StepNumber == 3)
                {
                    _logger.LogDebug($"Step {fileData.StepNumber}");
                    // Estate, Block and Hackney Totals 
                    var blockGroup = excelData.GroupBy(x => x.BlockId).ToList();
                    var estateGroup = excelData.GroupBy(x => x.EstateId).ToList();

                    var blockCharges = await GetSummarisedChargesList(blockGroup, _blockFullList, AssetType.Block.ToString(),
                       ChargeGroup.Leaseholders, chargeSubGroup, Constants.ChargesListenerUserName, chargeYear);
                    _logger.LogDebug($"Block Charges formation Process completed with total record count as : {blockCharges.Count()}");

                    var estateCharges = await GetSummarisedChargesList(estateGroup, _estateFullList, AssetType.Estate.ToString(),
                       ChargeGroup.Leaseholders, chargeSubGroup, Constants.ChargesListenerUserName, chargeYear);
                    _logger.LogDebug($"Estate Charges formation Process completed with total record count as : {estateCharges.Count()}");

                    var hackneyTotalCharge = GetHackneyTotal(excelData, AssetType.NA.ToString(),
                       ChargeGroup.Leaseholders, chargeSubGroup, Constants.ChargesListenerUserName, chargeYear);
                    _logger.LogDebug($"Hackney Total Charges formation Process completed");

                    _logger.LogDebug($"Block, Estate, Hackney Charges Write Starting");
                    var writeResult = await _chargesApiGateway.SaveBatchAsync(blockCharges).ConfigureAwait(false); //await WriteChargeItems(blockCharges).ConfigureAwait(false);
                    if (writeResult)
                        writeResult = await _chargesApiGateway.SaveBatchAsync(estateCharges).ConfigureAwait(false); //await WriteChargeItems(estateCharges).ConfigureAwait(false);
                    if (writeResult)
                        await _chargesApiGateway.AddChargeAsync(hackneyTotalCharge).ConfigureAwait(false);
                    _logger.LogDebug($"Block, Estate, Hackney Charges Write Starting");

                    if (writeResult)
                        await PushMessageToSNS(fileData).ConfigureAwait(false);
                }

                // Get Excel Data
                // Group By Block Id
                // Get Block Summaries list
                // Write Block Summaries List
                if (fileData.StepNumber == 4)
                {
                    _logger.LogDebug($"Step {fileData.StepNumber}");
                    if (excelData != null)
                    {
                        // Estate, Block and Hackney Totals 
                        var blockGroup = excelData.GroupBy(x => x.BlockId).ToList();
                        // Financial Summary Load
                        // Block Summary Load
                        var blockSummaries = GetAssetSummariesByType(blockGroup, _blockFullList, excelData, TargetType.Block, chargeYear, chargeSubGroup);
                        var blockSummaryLoadResult = await _financialSummaryService.AddEstimateActualSummaryBatch(blockSummaries.ToList()).ConfigureAwait(false);

                        if (blockSummaryLoadResult)
                            await PushMessageToSNS(fileData).ConfigureAwait(false);
                    }
                }

                // Get Excel Data
                // Group By Estate Id
                // Get Estate Summaries list
                // Write Estate Summaries List
                // Write Hackney Total Sumamry
                // Update File Tag to Processed
                if (fileData.StepNumber == 5)
                {
                    _logger.LogDebug($"Step {fileData.StepNumber}");
                    if (excelData != null)
                    {
                        // Estate, Block and Hackney Totals 
                        var estateGroup = excelData.GroupBy(x => x.EstateId).ToList();
                        // Financial Summary Load
                        // Estate Summary Load
                        var estateSumaries = GetAssetSummariesByType(estateGroup, _estateFullList, excelData, TargetType.Estate, chargeYear, chargeSubGroup);
                        var estateSummaryLoadResult = await _financialSummaryService.AddEstimateActualSummaryBatch(estateSumaries.ToList()).ConfigureAwait(false);

                        // Hackney Total Summary Load
                        var freeholdersCount = Helper.GetFreeholdersCount(excelData);
                        var leaseholdersCount = Helper.GetLeaseholdersCount(excelData);
                        var totalEstimateCharge = Convert.ToInt32(excelData.Sum(x => x.TotalCharge));

                        var addSummaryRequest = new AddAssetSummaryRequest
                        {
                            TargetId = Guid.Parse(Constants.HackneyRootAssetId),
                            SubmitDate = DateTime.UtcNow,
                            AssetName = Constants.RootAsset,
                            SumamryYear = chargeYear,
                            TargetType = TargetType.NA,
                            TotalServiceCharges = totalEstimateCharge,
                            TotalDwellings = excelData.Count(),
                            TotalFreeholders = freeholdersCount,
                            TotalLeaseholders = leaseholdersCount,
                            ValuesType = Enum.Parse<ValuesType>(chargeSubGroup)
                        };
                        var loadSummaryResult = await _financialSummaryService.AddEstimateSummary(addSummaryRequest).ConfigureAwait(false);

                        _logger.LogDebug($"Charges Summaries loading Process completed with total record count loaded : {excelData.Count}");

                        // Update File tag status
                        var updateTagResponse = await _awsS3FileService.UpdateFileTag(bucketName, fileData.RelativePath, Constants.SuccessfulProcessingTagValue).ConfigureAwait(false);
                        if (updateTagResponse)
                            _logger.LogDebug($"Estimate or Actual file processed successfully");

                    }
                }
                //var response = await _awsS3FileService.GetFile(bucketName, fileData.RelativePath).ConfigureAwait(false);

                //if (response != null)
                //{
                //    try
                //    {
                //        var recordsCount = 0;

                //        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                //        var estimatesActual = new List<EstimateActualCharge>();
                //        // Read Excel
                //        using (var stream = new MemoryStream())
                //        {
                //            response.CopyTo(stream);
                //            stream.Position = 1;

                //            // Excel Read Process
                //            using var reader = ExcelReaderFactory.CreateReader(stream);

                //            while (reader.Read())
                //            {
                //                if (recordsCount == 0)
                //                {
                //                    chargeYear = Convert.ToInt16($"20{reader.GetValue(19).ToString().Substring(0, 2)}");
                //                    chargeSubGroup = reader.GetValue(19).ToString().Substring(0, 3).EndsWith("E")
                //                               ? Constants.EstimateTypeFile
                //                               : Constants.ActualTypeFile;

                //                    _logger.LogDebug($"Extracted the ChargeYear for Estimates Upload as {chargeYear}");
                //                }
                //                else
                //                {
                //                    try
                //                    {
                //                        estimatesActual.Add(new EstimateActualCharge
                //                        {
                //                            PropertyReferenceNumber = reader.GetValue(1).ToString(),
                //                            AssetAddress = reader.GetValue(2).ToString(),
                //                            TenureType = reader.GetValue(3).ToString(),
                //                            BlockId = reader.GetValue(4).ToString(),
                //                            BlockAddress = reader.GetValue(5).ToString(),
                //                            EstateId = reader.GetValue(6).ToString(),
                //                            EstateAddress = reader.GetValue(7).ToString(),
                //                            TotalCharge = GetChargeAmount(reader.GetValue(18)),
                //                            BlockCCTVMaintenanceAndMonitoring = GetChargeAmount(reader.GetValue(19)),
                //                            BlockCleaning = GetChargeAmount(reader.GetValue(20)),
                //                            BlockElectricity = GetChargeAmount(reader.GetValue(21)),
                //                            BlockRepairs = GetChargeAmount(reader.GetValue(22)),
                //                            BuildingInsurancePremium = GetChargeAmount(reader.GetValue(23)),
                //                            DoorEntry = GetChargeAmount(reader.GetValue(24)),
                //                            CommunalTVAerialMaintenance = GetChargeAmount(reader.GetValue(25)),
                //                            ConciergeService = GetChargeAmount(reader.GetValue(26)),
                //                            EstateCCTVMaintenanceAndMonitoring = GetChargeAmount(reader.GetValue(27)),
                //                            EstateCleaning = GetChargeAmount(reader.GetValue(28)),
                //                            EstateElectricity = GetChargeAmount(reader.GetValue(29)),
                //                            EstateRepairs = GetChargeAmount(reader.GetValue(30)),
                //                            EstateRoadsFootpathsAndDrainage = GetChargeAmount(reader.GetValue(31)),
                //                            GroundRent = GetChargeAmount(reader.GetValue(32)),
                //                            GroundsMaintenance = GetChargeAmount(reader.GetValue(33)),
                //                            HeatingOrHotWaterEnergy = GetChargeAmount(reader.GetValue(34)),
                //                            HeatingOrHotWaterMaintenance = GetChargeAmount(reader.GetValue(35)),
                //                            HeatingStandingCharge = GetChargeAmount(reader.GetValue(36)),
                //                            LiftMaintenance = GetChargeAmount(reader.GetValue(37)),
                //                            ManagementCharge = GetChargeAmount(reader.GetValue(38)),
                //                            ReserveFund = GetChargeAmount(reader.GetValue(39))
                //                        });
                //                    }
                //                    catch (Exception e)
                //                    {
                //                        //_logger.LogDebug($"Exception occurred while reading the Estimates Excel Sheet: {e.Message}");
                //                        throw new Exception(e.Message);
                //                    }
                //                }
                //                recordsCount++;
                //            }
                //        }

                //        // Load Assets, Blocks , EStates 
                //        _logger.LogDebug($"Starting fetching assets list from Housing Search API Asset Search Endpoint");
                //        var assetsList = await GetAssetsList(AssetType.Dwelling.ToString()).ConfigureAwait(false);
                //        var dwellingsListResult = assetsList.Item1;

                //        //var blockList = await GetAssetsList(AssetType.Block.ToString()).ConfigureAwait(false);
                //        //var blockListResult = blockList.Item1;

                //        //var estateList = await GetAssetsList(AssetType.Estate.ToString()).ConfigureAwait(false);
                //        //var estateListResult = estateList.Item1;

                //        _logger.LogDebug($"Assets List fetching completed and total assets fetched : {assetsList.Item1.Count}");



                //        // Dwelling Charges Transformation
                //        _logger.LogDebug($"Starting UH numerical Asset Id transformation with Guid Asset Id");
                //        estimatesActual.ForEach(item =>
                //        {
                //            var data = dwellingsListResult.FirstOrDefault(x => x.AssetId == item.PropertyReferenceNumber);
                //            // TBC
                //            if (data == null)
                //            {
                //                _logger.LogDebug($"Could not find associated Guid Asset Id for UH Asset Id : {item.PropertyReferenceNumber}");
                //                item.AssetId = Guid.NewGuid();
                //                _logger.LogDebug($"Created Asset Guid for UH Asset Id {item.PropertyReferenceNumber}: {item.AssetId}");
                //            }
                //            else
                //                item.AssetId = data.Id;
                //        });

                //        // Property Charges
                //        _logger.LogDebug($"Starting Charges formation Process");
                //        var propertyCharges = new List<Charge>();
                //        var createdBy = Constants.ChargesListenerUserName;
                //        foreach (var item in estimatesActual)
                //        {
                //            propertyCharges.Add(ChargeHelper.GetChargeModel(AssetType.Dwelling.ToString(),
                //                ChargeGroup.Leaseholders, chargeSubGroup, createdBy, chargeYear, item));
                //        }

                //        // Estate, Block and Hackney Totals 
                //        var blockGroup = estimatesActual.GroupBy(x => x.BlockId).ToList();
                //        var estateGroup = estimatesActual.GroupBy(x => x.EstateId).ToList();

                //        var blockCharges = await GetSummarisedChargesList(blockGroup, _blockFullList, AssetType.Block.ToString(),
                //            ChargeGroup.Leaseholders, chargeSubGroup, createdBy, chargeYear);

                //        var estateCharges = await GetSummarisedChargesList(estateGroup, _estateFullList, AssetType.Estate.ToString(),
                //            ChargeGroup.Leaseholders, chargeSubGroup, createdBy, chargeYear);

                //        var hackneyTotalCharge = GetHackneyTotal(estimatesActual, AssetType.NA.ToString(),
                //            ChargeGroup.Leaseholders, chargeSubGroup, createdBy, chargeYear);
                //        _logger.LogDebug($"Charges formation Process completed with total record count as : {propertyCharges.Count}");


                //        // Charges Load
                //        var writeResult = await WriteChargeItems(propertyCharges).ConfigureAwait(false);
                //        if (writeResult)
                //            writeResult = await WriteChargeItems(blockCharges).ConfigureAwait(false);
                //        if (writeResult)
                //            writeResult = await WriteChargeItems(estateCharges).ConfigureAwait(false);
                //        if (writeResult)
                //            await _chargesApiGateway.AddChargeAsync(hackneyTotalCharge).ConfigureAwait(false);


                //        // Financial Summary Load
                //        // Block Summary Load
                //        var blockSummaries = GetAssetSummariesByType(blockGroup, _blockFullList, estimatesActual, TargetType.Block, chargeYear, chargeSubGroup);
                //        var blockSummaryLoadResult = await _financialSummaryService.AddEstimateActualSummaryBatch(blockSummaries.ToList()).ConfigureAwait(false);

                //        // Estate Summary Load
                //        var estateSumaries = GetAssetSummariesByType(estateGroup, _estateFullList, estimatesActual, TargetType.Estate, chargeYear, chargeSubGroup);
                //        var estateSummaryLoadResult = await _financialSummaryService.AddEstimateActualSummaryBatch(estateSumaries.ToList()).ConfigureAwait(false);

                //        // Hackney Total Summary Load
                //        var freeholdersCount = Helper.GetFreeholdersCount(estimatesActual);
                //        var leaseholdersCount = Helper.GetLeaseholdersCount(estimatesActual);
                //        var totalEstimateCharge = Convert.ToInt32(estimatesActual.Sum(x => x.TotalCharge));

                //        var addSummaryRequest = new AddAssetSummaryRequest
                //        {
                //            TargetId = Guid.Parse(Constants.HackneyRootAssetId),
                //            SubmitDate = DateTime.UtcNow,
                //            AssetName = Constants.RootAsset,
                //            SumamryYear = chargeYear,
                //            TargetType = TargetType.NA,
                //            TotalServiceCharges = totalEstimateCharge,
                //            TotalDwellings = estimatesActual.Count,
                //            TotalFreeholders = freeholdersCount,
                //            TotalLeaseholders = leaseholdersCount,
                //            ValuesType = Enum.Parse<ValuesType>(chargeSubGroup)
                //        };
                //        var loadSummaryResult = await _financialSummaryService.AddEstimateSummary(addSummaryRequest).ConfigureAwait(false);

                //        _logger.LogDebug($"Charges loading Process completed with total record count loaded : {estimatesActual.Count}");

                //        // Update File tag status
                //        var updateTagResponse = await _awsS3FileService.UpdateFileTag(bucketName, fileData.RelativePath, Constants.SuccessfulProcessingTagValue).ConfigureAwait(false);
                //        if (updateTagResponse)
                //            _logger.LogDebug($"Estimate or Actual file processed successfully");
                //    }
                //    catch (Exception ex)
                //    {
                //        //var length = ex.Message.Length > 254 ? 254 : (ex.Message.Length - 1);
                //        //await _awsS3FileService.UpdateFileTag(bucketName, fileData.RelativePath, ex.Message.Substring(0, length)).ConfigureAwait(false);
                //        throw new Exception($"Failed to process the file {ex.Message}", ex.InnerException);
                //    }

                //}
                //else
                //    _logger.LogError($"Failed to process the Estimate or Actual file");
            }

        }
        //private static (short, string) GetFileType(Stream s3FileData)
        //{
        //    var recordsCount = 0;
        //    short chargeYear = 0;
        //    var chargeSubGroup = string.Empty;

        //    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        //    // Read Excel
        //    using (var stream = new MemoryStream())
        //    {
        //        s3FileData.CopyTo(stream);
        //        stream.Position = 1;

        //        // Excel Read Process
        //        using var reader = ExcelReaderFactory.CreateReader(stream);

        //        while (reader.Read())
        //        {
        //            if (recordsCount != 0 && reader.GetValue(1) != null)
        //            {
        //                try
        //                {
        //                    if (recordsCount == 0)
        //                    {
        //                        chargeYear = Convert.ToInt16($"20{reader.GetValue(19).ToString().Substring(0, 2)}");
        //                        chargeSubGroup = reader.GetValue(19).ToString().Substring(0, 3).EndsWith("E")
        //                                   ? Constants.EstimateTypeFile
        //                                   : Constants.ActualTypeFile;
        //                        break;
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    throw new Exception(e.Message);
        //                }
        //            }
        //            recordsCount++;
        //        }
        //    }
        //    return (chargeYear, chargeSubGroup);
        //}
        //private static List<EstimateActualCharge> GetExcelData(Stream s3FileData)
        //{

        //    var recordsCount = 0;

        //    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        //    var estimatesActual = new List<EstimateActualCharge>();
        //    // Read Excel
        //    using (var stream = new MemoryStream())
        //    {
        //        s3FileData.CopyTo(stream);
        //        stream.Position = 1;

        //        // Excel Read Process
        //        using var reader = ExcelReaderFactory.CreateReader(stream);

        //        while (reader.Read())
        //        {
        //            if (recordsCount != 0 && reader.GetValue(1) != null)
        //            {
        //                try
        //                {
        //                    estimatesActual.Add(new EstimateActualCharge
        //                    {
        //                        PropertyReferenceNumber = reader.GetValue(1).ToString(),
        //                        AssetAddress = reader.GetValue(2).ToString(),
        //                        TenureType = reader.GetValue(3).ToString(),
        //                        BlockId = reader.GetValue(4).ToString(),
        //                        BlockAddress = reader.GetValue(5).ToString(),
        //                        EstateId = reader.GetValue(6).ToString(),
        //                        EstateAddress = reader.GetValue(7).ToString(),
        //                        TotalCharge = GetChargeAmount(reader.GetValue(18)),
        //                        BlockCCTVMaintenanceAndMonitoring = GetChargeAmount(reader.GetValue(19)),
        //                        BlockCleaning = GetChargeAmount(reader.GetValue(20)),
        //                        BlockElectricity = GetChargeAmount(reader.GetValue(21)),
        //                        BlockRepairs = GetChargeAmount(reader.GetValue(22)),
        //                        BuildingInsurancePremium = GetChargeAmount(reader.GetValue(23)),
        //                        DoorEntry = GetChargeAmount(reader.GetValue(24)),
        //                        CommunalTVAerialMaintenance = GetChargeAmount(reader.GetValue(25)),
        //                        ConciergeService = GetChargeAmount(reader.GetValue(26)),
        //                        EstateCCTVMaintenanceAndMonitoring = GetChargeAmount(reader.GetValue(27)),
        //                        EstateCleaning = GetChargeAmount(reader.GetValue(28)),
        //                        EstateElectricity = GetChargeAmount(reader.GetValue(29)),
        //                        EstateRepairs = GetChargeAmount(reader.GetValue(30)),
        //                        EstateRoadsFootpathsAndDrainage = GetChargeAmount(reader.GetValue(31)),
        //                        GroundRent = GetChargeAmount(reader.GetValue(32)),
        //                        GroundsMaintenance = GetChargeAmount(reader.GetValue(33)),
        //                        HeatingOrHotWaterEnergy = GetChargeAmount(reader.GetValue(34)),
        //                        HeatingOrHotWaterMaintenance = GetChargeAmount(reader.GetValue(35)),
        //                        HeatingStandingCharge = GetChargeAmount(reader.GetValue(36)),
        //                        LiftMaintenance = GetChargeAmount(reader.GetValue(37)),
        //                        ManagementCharge = GetChargeAmount(reader.GetValue(38)),
        //                        ReserveFund = GetChargeAmount(reader.GetValue(39))
        //                    });
        //                }
        //                catch (Exception e)
        //                {
        //                    throw new Exception(e.Message);
        //                }
        //            }
        //            recordsCount++;
        //        }
        //    }
        //    return estimatesActual;
        //}
        private async Task PushMessageToSNS(EntityFileMessageSqs fileData, int writeIndex = 0, bool toNextStep = true)
        {
            var messageToPublish = fileData;
            messageToPublish.StepNumber = toNextStep ? fileData.StepNumber + 1 : fileData.StepNumber;
            messageToPublish.WriteIndex = writeIndex;
            var publishMessage = ChargesSnsFactory.CreateFileUploadMessage(messageToPublish);
            await _snsGateway.Publish(publishMessage).ConfigureAwait(false);
        }

        private async Task<bool> WriteChargeItems(List<Charge> charges)
        {
            var maxBatchCount = Constants.PerBatchProcessingCount;
            bool loadResult = false;
            var loopCount = 0;
            if (charges.Count % maxBatchCount == 0)
                loopCount = charges.Count / maxBatchCount;
            else
                loopCount = (charges.Count / maxBatchCount) + 1;


            for (var start = 0; start < loopCount; start++)
            {
                var itemsToWrite = charges.Skip(start * maxBatchCount).Take(maxBatchCount);
                loadResult = await _chargesApiGateway.AddTransactionBatchAsync(itemsToWrite.ToList()).ConfigureAwait(false);
                if (!loadResult)
                    throw new Exception("Something wrong happend while writing charges");
                Thread.Sleep(1000);
            }
            return loadResult;
        }

        private List<AddAssetSummaryRequest> GetAssetSummariesByType(List<IGrouping<string, EstimateActualCharge>> assetsGroup,
          List<Asset> assetListResult, List<EstimateActualCharge> excelData, TargetType targetType, short chargeYear, string type)
        {
            var summaries = new List<AddAssetSummaryRequest>();
            foreach (var item in assetsGroup)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    var items = item.Select(x => x).ToList();

                    var totalBlockGroup = items.GroupBy(x => x.BlockId).Where(c => !string.IsNullOrEmpty(c.Key)).ToList();

                    var totalLeaseholderCount = Helper.GetLeaseholdersCount(items);
                    var totalFreeholderCount = Helper.GetFreeholdersCount(items);
                    var totalDwellingCount = items.Count();
                    var totalCharge = items.Sum(x => x.TotalCharge);
                    var groupAsset = assetListResult.FirstOrDefault(x => x.AssetId == item.Key);
                    var id = Guid.NewGuid();
                    if (groupAsset != null)
                    {
                        id = groupAsset.Id;
                    }
                    var assetName = targetType == TargetType.Block
                        ? excelData.FirstOrDefault(x => x.BlockId == item.Key)?.BlockAddress
                        : excelData.FirstOrDefault(x => x.BlockId == item.Key)?.EstateAddress;
                    var addSummary = new AddAssetSummaryRequest
                    {
                        TargetId = id,
                        SubmitDate = DateTime.UtcNow,
                        AssetName = assetName,
                        SumamryYear = chargeYear,
                        TargetType = targetType,
                        TotalServiceCharges = Math.Round(totalCharge, 2),
                        TotalDwellings = totalDwellingCount,
                        TotalFreeholders = totalFreeholderCount,
                        TotalLeaseholders = totalLeaseholderCount,
                        TotalBlocks = targetType == TargetType.Estate ? totalBlockGroup.Count : 0,
                        ValuesType = Enum.Parse<ValuesType>(type)
                    };
                    summaries.Add(addSummary);
                }
            }
            return summaries;
        }

        private async Task<List<Charge>> GetSummarisedChargesList(List<IGrouping<string, EstimateActualCharge>> assetsGroup,
           List<Asset> assetListResult, string assetType, ChargeGroup chargeGroup, string chargeSubGroup,
           string createdBy, short chargeYear)
        {
            var chargesResultList = new List<Charge>();
            foreach (var item in assetsGroup)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    var groupAsset = assetListResult.FirstOrDefault(x => x.AssetId == item.Key);
                    var id = Guid.NewGuid();
                    if (groupAsset != null)
                    {
                        id = groupAsset.Id;
                    }
                    else
                    {
                        // Asset Info API Call
                        var numericalAssetId = item.Key;
                        if (item.Key.Length < 8)
                        {
                            numericalAssetId = numericalAssetId.PadLeft(8, '0');
                        }

                        // SCAN ASSET Table
                        var assetDetails = await _assetInformationApiGateway.GetAssetByAssetIdAsync(numericalAssetId).ConfigureAwait(false);
                        if (assetDetails != null)
                            id = assetDetails.Id;
                    }

                    var groupItem = new EstimateActualCharge
                    {
                        AssetId = id,
                        BlockCCTVMaintenanceAndMonitoring = item.Sum(x => x.BlockCCTVMaintenanceAndMonitoring),
                        BlockCleaning = item.Sum(x => x.BlockCleaning),
                        BlockElectricity = item.Sum(x => x.BlockElectricity),
                        BlockRepairs = item.Sum(x => x.BlockRepairs),
                        BuildingInsurancePremium = item.Sum(x => x.BuildingInsurancePremium),
                        CommunalTVAerialMaintenance = item.Sum(x => x.CommunalTVAerialMaintenance),
                        ConciergeService = item.Sum(x => x.ConciergeService),
                        DoorEntry = item.Sum(x => x.DoorEntry),
                        EstateCCTVMaintenanceAndMonitoring = item.Sum(x => x.EstateCCTVMaintenanceAndMonitoring),
                        EstateCleaning = item.Sum(x => x.EstateCleaning),
                        EstateElectricity = item.Sum(x => x.EstateElectricity),
                        EstateRepairs = item.Sum(x => x.EstateRepairs),
                        EstateRoadsFootpathsAndDrainage = item.Sum(x => x.EstateRoadsFootpathsAndDrainage),
                        GroundRent = item.Sum(x => x.GroundRent),
                        GroundsMaintenance = item.Sum(x => x.GroundsMaintenance),
                        HeatingOrHotWaterEnergy = item.Sum(x => x.HeatingOrHotWaterEnergy),
                        HeatingOrHotWaterMaintenance = item.Sum(x => x.HeatingOrHotWaterMaintenance),
                        HeatingStandingCharge = item.Sum(x => x.HeatingStandingCharge),
                        LiftMaintenance = item.Sum(x => x.LiftMaintenance),
                        ManagementCharge = item.Sum(x => x.ManagementCharge),
                        ReserveFund = item.Sum(x => x.ReserveFund)
                    };

                    chargesResultList.Add(ChargeHelper.GetChargeModel(assetType,
                        ChargeGroup.Leaseholders, chargeSubGroup, createdBy, chargeYear, groupItem));
                }
            }
            return chargesResultList;
        }
        private static Charge GetHackneyTotal(List<EstimateActualCharge> items, string assetType,
            ChargeGroup chargeGroup, string chargeSubGroup, string createdBy, short chargeYear)
        {
            var totalItem = new EstimateActualCharge
            {
                AssetId = Guid.Parse(Constants.HackneyRootAssetId),
                BlockCCTVMaintenanceAndMonitoring = items.Sum(x => x.BlockCCTVMaintenanceAndMonitoring),
                BlockCleaning = items.Sum(x => x.BlockCleaning),
                BlockElectricity = items.Sum(x => x.BlockElectricity),
                BlockRepairs = items.Sum(x => x.BlockRepairs),
                BuildingInsurancePremium = items.Sum(x => x.BuildingInsurancePremium),
                CommunalTVAerialMaintenance = items.Sum(x => x.CommunalTVAerialMaintenance),
                ConciergeService = items.Sum(x => x.ConciergeService),
                DoorEntry = items.Sum(x => x.DoorEntry),
                EstateCCTVMaintenanceAndMonitoring = items.Sum(x => x.EstateCCTVMaintenanceAndMonitoring),
                EstateCleaning = items.Sum(x => x.EstateCleaning),
                EstateElectricity = items.Sum(x => x.EstateElectricity),
                EstateRepairs = items.Sum(x => x.EstateRepairs),
                EstateRoadsFootpathsAndDrainage = items.Sum(x => x.EstateRoadsFootpathsAndDrainage),
                GroundRent = items.Sum(x => x.GroundRent),
                GroundsMaintenance = items.Sum(x => x.GroundsMaintenance),
                HeatingOrHotWaterEnergy = items.Sum(x => x.HeatingOrHotWaterEnergy),
                HeatingOrHotWaterMaintenance = items.Sum(x => x.HeatingOrHotWaterMaintenance),
                HeatingStandingCharge = items.Sum(x => x.HeatingStandingCharge),
                LiftMaintenance = items.Sum(x => x.LiftMaintenance),
                ManagementCharge = items.Sum(x => x.ManagementCharge),
                ReserveFund = items.Sum(x => x.ReserveFund)
            };
            return ChargeHelper.GetChargeModel(assetType,
                        chargeGroup, chargeSubGroup, createdBy, chargeYear, totalItem);
        }

        private static decimal GetChargeAmount(object excelColumnValue)
        {
            decimal result;
            if (excelColumnValue == null || excelColumnValue.ToString() == " ")
                result = 0;
            else
                result = Convert.ToDecimal(excelColumnValue);
            return result;
        }
        private async Task<(List<Asset>, long)> GetAssetsList(string assetType)
        {
            var pageNumber = Constants.Page;
            var pageSize = Constants.PageSize;

            var filteredList = new List<Asset>();

            var assetList = await _housingSearchService.GetAssets(assetType, pageSize, pageNumber, null)
                .ConfigureAwait(false);

            if (assetList is null) throw new Exception("Housing Search Service is not returning any asset list response");

            var totalPropertiesCount = assetList.Total;

            var lastHitId = assetList.LastHitId;

            filteredList.AddRange(assetList.Results.Assets);

            for (var i = 0; i < (assetList.Total / pageSize); i++)
            {
                pageNumber = i + 2;
                assetList = await _housingSearchService
                    .GetAssets(assetType, pageSize, pageNumber, lastHitId).ConfigureAwait(false);
                if (assetList is null) throw new Exception("Housing Search Service is not returning any asset list response");
                lastHitId = assetList.LastHitId;
                filteredList.AddRange(assetList.Results.Assets);
            }

            return (filteredList, totalPropertiesCount);
        }
    }
}
