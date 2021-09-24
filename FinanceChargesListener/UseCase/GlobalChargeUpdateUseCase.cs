using FinanceChargesListener.Boundary;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Infrastructure.Exceptions;
using FinanceChargesListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase
{
    public class GlobalChargeUpdateUseCase : IGlobalChargeUpdateUseCase
    {
        private readonly IAssetInformationApiGateway _assetInformationApiGateway;
        private readonly IChargesApiGateway _chargesApiGateway;
        private readonly IChargesMaintenanceApiGateway _chargesMaintenanceApiGateway;

        public GlobalChargeUpdateUseCase(
            IAssetInformationApiGateway assetInformationApiGateway,
            IChargesApiGateway chargesApiGateway,
            IChargesMaintenanceApiGateway chargesMaintenanceApiGateway)
        {
            _assetInformationApiGateway = assetInformationApiGateway;
            _chargesApiGateway = chargesApiGateway;
            _chargesMaintenanceApiGateway = chargesMaintenanceApiGateway;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            var assetDetailList = await _assetInformationApiGateway.GetEstateBlockList().ConfigureAwait(false);
            if (assetDetailList is null) throw new EntityNotFoundException<AssetList>(message.EntityId);

            // Loop Through the Id List
            if (assetDetailList != null && assetDetailList.AssetDetails.Any())
            {
                foreach (var item in assetDetailList.AssetDetails)
                {
                    var charge = await _chargesApiGateway.GetChargeByTargetIdAsync(item.Id).ConfigureAwait(false);
                    var updateChargeResponse = new Charge();
                    if (charge is null)
                    {
                        var newCharge = new AddCharge();
                        updateChargeResponse = await _chargesApiGateway.AddCharge(newCharge).ConfigureAwait(false);
                    }
                    else
                    {
                        var updateCharge = new Charge();
                        updateChargeResponse = await _chargesApiGateway.UpdateChargeAsync(updateCharge).ConfigureAwait(false);
                    }

                    if (updateChargeResponse != null)
                    {
                        var chargeMaintenance = new ChargeMaintenance();
                        var chargeMaintenanceResponse = await _chargesMaintenanceApiGateway.AddChargeMaintenance(chargeMaintenance).ConfigureAwait(false);
                        if (chargeMaintenanceResponse != null)
                            continue;
                        else
                            break;
                    }
                }
            }
            
        }
    }
}
