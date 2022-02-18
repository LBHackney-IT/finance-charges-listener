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

            foreach (var updatedChargeDetail in updatedDetailedCharges)
            {
                var existingDetailsToUpdate = chargesDetails
                    .Where(cd => cd.SubType == updatedChargeDetail.SubType &&
                                    cd.ChargeType == updatedChargeDetail.ChargeType)
                    .ToList();

                existingDetailsToUpdate.ForEach(cd =>
                {
                    cd.Amount = updatedChargeDetail.NewAmount;
                });
            }

            await _chargesGateway.SaveBatchAsync(chargesToUpdate.ToList());

            var assetSummary = await _summaryApiHttpClient.GetAssetEstimate(chargeEventMessage.EntityTargetId);

            // Hanna Holasava
            // What is no asset esimate summary was fount?
            // Should we create new one?
            if (assetSummary == null)
            {
                return;
            }
            decimal newTotalServiceCharges = assetSummary.TotalServiceCharges + GetServiceChargeDifference(updatedCharge, updatedDetailedCharges);

            await _summaryApiHttpClient.UpdateTotalServiceCharges(chargeEventMessage.EntityTargetId, newTotalServiceCharges);
        }

        private decimal GetServiceChargeDifference(Charge existingModel, List<DetailedChargeChange> detailedChargesToUpdate)
        {
            return existingModel.DetailedCharges
                .Join(detailedChargesToUpdate,
                    outer => new { outer.SubType, outer.ChargeType },
                    inner => new { inner.SubType, inner.ChargeType },
                    (old, updated) => new { OldAmout = old.Amount, NewAmount = updated.NewAmount })
                .Sum(_ => _.NewAmount - _.OldAmout);
        }
    }
}
