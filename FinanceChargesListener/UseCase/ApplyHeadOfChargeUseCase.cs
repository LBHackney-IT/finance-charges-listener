using FinanceChargesListener.Boundary;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Gateway.Services.Interfaces;
using FinanceChargesListener.Infrastructure.Exceptions;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Hackney.Shared.Tenure.Domain;
using static System.String;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;
using FinanceChargesListener.UseCase.Interfaces;

namespace FinanceChargesListener.UseCase
{
    public class ApplyHeadOfChargeUseCase : Interfaces.ApplyHeadOfChargeUseCase
    {
        private readonly HousingSearchService _housingSearchService;
        private readonly IChargesApiGateway _chargesApiGateway;
        private readonly Gateway.Services.Interfaces.IAssetInformationApiGateway _assetInformationApiGateway;
        private readonly AssetGateway _assetGateway;
        private readonly ChargesMaintenanceApiGateway _chargesMaintenanceApiGateway;
        private readonly IManagementFeeUseCase _managementFeeUseCase;
        private readonly ICommonMethodUseCase _commonMethodUseCase;

        public ApplyHeadOfChargeUseCase(HousingSearchService housingSearchService,
            IChargesApiGateway chargesApiGateway,
            Gateway.Services.Interfaces.IAssetInformationApiGateway assetInformationApiGateway,
            AssetGateway assetGateway,
            ChargesMaintenanceApiGateway chargesMaintenanceApiGateway,
            IManagementFeeUseCase managementFeeUseCase,
            ICommonMethodUseCase commonMethodUseCase)
        {
            _housingSearchService = housingSearchService;
            _chargesApiGateway = chargesApiGateway;
            _assetInformationApiGateway = assetInformationApiGateway;
            _assetGateway = assetGateway;
            _chargesMaintenanceApiGateway = chargesMaintenanceApiGateway;
            _managementFeeUseCase = managementFeeUseCase;
            _commonMethodUseCase = commonMethodUseCase;
        }

        public async Task ProcessMessageAsync(EntityEventSns message, JsonSerializerOptions jsonSerializerOptions)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));
            var filteredList = await GetAssetsList(message, AssetType.Dwelling.ToString());

            var assetsWithBlockEstate = filteredList.Item1.Where(x => x.AssetLocation.ParentAssets.Any()).ToList();

            var leaseholdAssetsList = GetLeaseholdersAssetsList(assetsWithBlockEstate);
            var tenantAssetsList = GetTenantsAssetsList(assetsWithBlockEstate);

            if (message?.EventData?.NewData != null)
            {
                var headOfChargeData = JsonSerializer.Deserialize<EntityMessageSqs>(message?.EventData?.NewData?.ToString() ?? Empty, jsonSerializerOptions);

                bool useCaseResult = headOfChargeData.ChargeName switch
                {
                    HeadOfCharges.ManagementFee => await _managementFeeUseCase.ApplyManagementFee(assetsWithBlockEstate)
                                                .ConfigureAwait(false),
                    var x when
                    x == HeadOfCharges.BlockCleaning ||
                    x == HeadOfCharges.BlockElectricity ||
                    x == HeadOfCharges.BlockRepairs ||
                    x == HeadOfCharges.EstateCleaning ||
                    x == HeadOfCharges.EstateElectricity ||
                    x == HeadOfCharges.EstateRepairs ||
                    x == HeadOfCharges.GroundsMaintenance => await _commonMethodUseCase.ApportionCharge(assetsWithBlockEstate,
                     message, jsonSerializerOptions).ConfigureAwait(false),
                    _ => throw new ArgumentException(
                            $"Unknown event type: {headOfChargeData.ChargeName} on event : {message.EventData}")
                };
            }
        }
        private async Task<List<Asset>> GetFullAssetsList(List<Asset> assetSearchResult)
        {
            var result = new List<Asset>();
            var degree = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0));

            var block = new ActionBlock<Asset>(
                    async x =>
                    {
                        var response = await _assetGateway.GetAssetByIdAsync(x.Id).ConfigureAwait(true); ;

                        result.Add(response);
                    },
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = degree, // Parallelize on all cores
                    });
            foreach (var asset in assetSearchResult)
            {
                block.Post(asset);
            }

            block.Complete();
            await block.Completion;

            return result.Where(x => x != null).ToList(); ;
        }

        private async Task<(List<Asset>, long)> GetAssetsList(EntityEventSns message, string assetType)
        {
            var time = Stopwatch.StartNew();

            var pageNumber = Constants.Page;
            var pageSize = Constants.PageSize;

            var filteredList = new List<Asset>();

            var assetList = await _housingSearchService.GetAssets(assetType, pageSize, pageNumber, null)
                .ConfigureAwait(false);

            if (assetList is null) throw new EntityNotFoundException<Asset>(message.EntityId);

            var totalPropertiesCount = assetList.Total;

            var lastHitId = assetList.LastHitId;

            filteredList.AddRange(assetList.Results.Assets);

            //var fullAssetList = await GetFullAssetsList(assetList.Results.Assets);

            for (var i = 0; i < (assetList.Total / pageSize); i++)
            {
                pageNumber = i + 2;
                assetList = await _housingSearchService
                    .GetAssets(assetType, pageSize, pageNumber, lastHitId).ConfigureAwait(false);
                if (assetList is null) throw new EntityNotFoundException<Asset>(message.EntityId);
                lastHitId = assetList.LastHitId;
                filteredList.AddRange(assetList.Results.Assets);

                //var assetDetailList = await GetFullAssetsList(assetList.Results.Assets);
                //fullAssetList.AddRange(assetDetailList);
            }

            time.Stop();
            var elapsedTime = time.ElapsedMilliseconds;

            return (filteredList, totalPropertiesCount);
            //return (fullAssetList, totalPropertiesCount) ;
        }

        private static List<Asset> GetLeaseholdersAssetsList(List<Asset> assets)
        {
            var response = new List<Asset>();
            if (assets == null || !assets.Any()) return response;
            var filteredData = assets.Where(
                   x => x.Tenure?.Type == TenureTypes.LeaseholdRTB.Description
                || x.Tenure?.Type == TenureTypes.PrivateSaleLH.Description
                || x.Tenure?.Type == TenureTypes.SharedOwners.Description
                || x.Tenure?.Type == TenureTypes.Freehold.Description
                || x.Tenure?.Type == TenureTypes.FreeholdServ.Description
                || x.Tenure?.Type == TenureTypes.ShortLifeLse.Description
            );
            response.AddRange(filteredData);
            return response;

        }

        private static List<Asset> GetTenantsAssetsList(List<Asset> assets)
        {
            var response = new List<Asset>();
            if (assets == null || !assets.Any()) return response;
            var filteredData = assets.Where(
                x => x.Tenure?.Type == TenureTypes.Secure.Description
                     || x.Tenure?.Type == TenureTypes.Introductory.Description
                     || x.Tenure?.Type == TenureTypes.TenantAccFlat.Description
                     || x.Tenure?.Type == TenureTypes.TempAnnex.Description
                     || x.Tenure?.Type == TenureTypes.TempBAndB.Description
                     || x.Tenure?.Type == TenureTypes.MesneProfitAc.Description
            );
            response.AddRange(filteredData);
            return response;

        }

    }
}
