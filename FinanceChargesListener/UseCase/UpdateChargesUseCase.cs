using FinanceChargesListener.Boundary;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Domain.EventMessages;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Gateway.Services.Interfaces;
using FinanceChargesListener.Infrastructure.Interfaces;
using FinanceChargesListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase
{
    public class UpdateChargesUseCase : IUpdateChargesUseCase
    {
        private readonly IAssetInformationApiGateway _assetGateway;
        private readonly IChargesApiGateway _chargesGateway;
        private readonly IFinancialSummaryApiGateway _summaryApiHttpClient;

        public UpdateChargesUseCase(IAssetInformationApiGateway assetGateway,
                                    IChargesApiGateway chargesGateway,
                                    IFinancialSummaryApiGateway summaryApiHttpClient)
        {
            _assetGateway = assetGateway;
            _chargesGateway = chargesGateway;
            _summaryApiHttpClient = summaryApiHttpClient;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message, JsonSerializerOptions jsonSerializerOptions)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (!(message is ChargesEventSns chargeEventMessage))
            {
                throw new ArgumentException("Event message format is invalid!");
            }

            string s = message.EventData.NewData.ToString();
            List<DetailedChargeChange> updatedDetailedCharges = JsonConvert.DeserializeObject<List<DetailedChargeChange>>(s);

            var updatedCharge = await _chargesGateway.GetById(message.EntityId, chargeEventMessage.EntityTargetId)
                ?? throw new ArgumentException($"Cannot load charge entity from Charges DynamoDB for asset id: {chargeEventMessage.EntityTargetId} and chargeId: {message.EntityId}");

            var asset = await _assetGateway.GetAssetByIdAsync(chargeEventMessage.EntityTargetId)
                ?? throw new ArgumentException($"Cannot load asset information entity from AssetInformationAPI for asset id: {chargeEventMessage.EntityTargetId}");

            var parentAssets = asset.AssetLocation?.ParentAssets;

            if (parentAssets == null || !parentAssets.Any())
            {
                return;
            }

            var chargesToUpdate = Enumerable.Empty<Charge>();
            foreach (var parentAsset in parentAssets)
            {
                var chargesForParentAsset = await _chargesGateway.GetChargeByTargetIdAsync(parentAsset.Id).ConfigureAwait(false)
                    ?? throw new ArgumentException($"Cannot load asset information entity from AssetInformationAPI for parent asset id: {parentAsset.Id}");

                chargesToUpdate = chargesToUpdate.Concat
                (
                    chargesForParentAsset.Where(c => c.ChargeYear == updatedCharge.ChargeYear
                                                    && c.ChargeSubGroup == updatedCharge.ChargeSubGroup)
                );
            }

            if (chargesToUpdate == null || !chargesToUpdate.Any())
            {
                throw new ArgumentException($"Cannot load Charges from ChargesAPI for asset id: {asset.Id}");
            }

            var chargesDetails = chargesToUpdate.SelectMany(c => c.DetailedCharges);
            decimal totalSumaryDifference = updatedDetailedCharges.Sum(_ => _.DifferenceAmount);
            foreach (var updatedChargeDetail in updatedDetailedCharges)
            {
                var existingDetailsToUpdate = chargesDetails
                    .Where(cd => cd.SubType == updatedChargeDetail.SubType &&
                                    cd.ChargeType == updatedChargeDetail.ChargeType)
                    .ToList();

                existingDetailsToUpdate.ForEach(cd =>
                {
                    cd.Amount += updatedChargeDetail.DifferenceAmount;
                });
            }

            await _chargesGateway.SaveBatchAsync(chargesToUpdate.ToList());

            foreach (var item in chargesToUpdate)
            {
                var assetSummary = await _summaryApiHttpClient.GetAssetEstimate(item.TargetId, updatedCharge.ChargeYear, updatedCharge.ChargeSubGroup.ToString());
                if (assetSummary != null)
                {
                    var newTotalServiceCharges = assetSummary.TotalServiceCharges + totalSumaryDifference;

                    await _summaryApiHttpClient.UpdateTotalServiceCharges(item.TargetId, newTotalServiceCharges,
                        updatedCharge.ChargeYear, assetSummary.ValuesType.ToString());
                }
            }
        }
    }
}
