using FinanceChargesListener.Boundary;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Factories;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.UseCase.Interfaces;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FinanceChargesListener.Domain.Enums;

namespace FinanceChargesListener.UseCase
{
    public class ProcessLeaseholdChargesUseCase : IProcessLeaseholdChargesUseCase
    {
        private readonly ChargesApiGateway _chargesApiGateway;

        public ProcessLeaseholdChargesUseCase(ChargesApiGateway chargesApiGateway)
        {
            _chargesApiGateway = chargesApiGateway;
        }
        public async Task<bool> ProcessLeaseholderServiceCharges(List<Asset> assets, ChargeType chargeType, EntityMessageSqs entityMessageSqs)
        {
            var calculatedList = Utility.Helper.GetPropertiesWithPercentage(assets);
            if (chargeType == ChargeType.Estate)
            {
                var estateProperties = Utility.Helper.GetAssetsWithEstate(assets);
                await ApplyLeaseholdersCharges(entityMessageSqs, estateProperties, calculatedList, chargeType);
            }
            else if (chargeType == ChargeType.Block)
            {
                var blockProperties = Utility.Helper.GetAssetsWithBlock(assets);
                await ApplyLeaseholdersCharges(entityMessageSqs, blockProperties, calculatedList, chargeType);
            }
            else
            {
                await ApplyLeaseholdersCharges(entityMessageSqs, assets, calculatedList, chargeType);
            }
            return true;
        }
        private async Task ApplyLeaseholdersCharges(EntityMessageSqs entityMessageSqs, List<Asset> assets,
            Dictionary<Guid, double> calculatedList, ChargeType chargeType)
        {
            foreach (var asset in assets)
            {
                var charges = await _chargesApiGateway.GetChargeByTargetIdAsync(asset.Id).ConfigureAwait(false);
                var chargeAmount = Math.Round((((decimal) calculatedList[asset.Id] * entityMessageSqs.TotalEstimateAmount) / 100) / 12, 2);

                // PATCH LOGIC
                if (charges != null && charges.Any())
                {
                    var estimatedLeaseholdCharge = charges.FirstOrDefault(x => x.ChargeGroup == ChargeGroup.Leaseholders
                           && x.ChargeYear == DateTime.UtcNow.Year);
                    if (estimatedLeaseholdCharge != null && estimatedLeaseholdCharge.DetailedCharges.Any())
                    {
                        foreach (var item in estimatedLeaseholdCharge.DetailedCharges)
                        {
                            if (item.ChargeCode == entityMessageSqs.ChargeCode)
                            {
                                item.ChargeCode = entityMessageSqs.ChargeCode;
                                item.ChargeType = chargeType;
                                item.Amount = chargeAmount;
                            }
                        }
                        if (!estimatedLeaseholdCharge.DetailedCharges.Any(x => x.ChargeCode == entityMessageSqs.ChargeCode))
                        {
                            var chargeDetail = Utility.Helper.GetChargeDetailModel(chargeAmount, entityMessageSqs.ChargeName,
                                entityMessageSqs.ChargeCode, ChargeGroup.Tenants, chargeType);
                            estimatedLeaseholdCharge.DetailedCharges.ToList().Add(chargeDetail);
                        }
                    }

                    await _chargesApiGateway.UpdateChargeAsync(estimatedLeaseholdCharge).ConfigureAwait(false);

                    //var chargeMaintenance = new ChargeMaintenance();
                    //var chargeMaintenanceResponse = await _chargesMaintenanceApiGateway.AddChargeMaintenance(chargeMaintenance).ConfigureAwait(false);
                }
                else
                {
                    var chargeToAdd = Utility.Helper.GetChargeModel(asset.Id, asset.AssetType.ToString(), chargeAmount,
                        entityMessageSqs.ChargeName, entityMessageSqs.ChargeCode, Enums.ChargeGroup.Leaseholders, chargeType);

                    await _chargesApiGateway.AddChargeAsync(chargeToAdd).ConfigureAwait(false);
                }
            }
        }
    }
}
