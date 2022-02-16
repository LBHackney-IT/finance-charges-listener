using FinanceChargesListener.Boundary;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase
{
    public class UpdateChargesUseCase : IUpdateChargesUseCase
    {
        private readonly IAssetGateway _assetGateway;
        private readonly IChargesGateway _chargesGateway;

        public UpdateChargesUseCase(IAssetGateway assetGateway,
                                    IChargesGateway chargesGateway)
        {
            _assetGateway = assetGateway;
            _chargesGateway = chargesGateway;
        }

        [LogCall]
        public async Task ProcessMessageAsync(ChargesEventSns message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.EventData.NewData is DwellingEventRequest request)
            {
                var updatedCharge = await _chargesGateway.GetById(request.ChargeId, request.AssetId)
                    ?? throw new ArgumentException($"Cannot load charge entity from Charges DynamoDB for asset id: {request.AssetId} and chargeId: {request.ChargeId}");

                var asset = await _assetGateway.GetById(request.AssetId)
                    ?? throw new ArgumentException($"Cannot load asset information entity from AssetInformationAPI for asset id: {request.AssetId}");

                var parentAssets = asset.AssetLocation?.ParentAssets;
                if (parentAssets == null)
                {
                    return;
                }

                foreach (var parentAsset in parentAssets)
                {
                    var chargesForParentAsset = await _chargesGateway.GetAllByAssetId(parentAsset.Id)
                        ?? throw new ArgumentException($"Cannot load asset information entity from AssetInformationAPI for parent asset id: {parentAsset.Id}");

                    var chargetToUpdate = chargesForParentAsset.Where(c => c.ChargeYear == updatedCharge.ChargeYear)

                }

                if (charges == null || charges.Count == 0)
                {
                    throw new ArgumentException($"Cannot load Charges from ChargesAPI for asset id: {asset.Id}");
                }

                var chargesDetails = charges.Where(c => c.ChargeGroup == request.cha).SelectMany(c => c.DetailedCharges);

                foreach (var updatedChargeDetail in request.Details)
                {
                    var existingDetailsToUpdate = chargesDetails
                        .Where(cd => cd.SubType == updatedChargeDetail.SubType &&
                                     cd.ChargeType == updatedChargeDetail.Cha)
                                                       .ToList();

                    existingDetailsToUpdate.ForEach(cd => cd.Amount = updatedChargeDetail.Amount);
                }

                // update in db
            }

            throw new ArgumentException(nameof(message.EventData.NewData));
        }
    }
}
