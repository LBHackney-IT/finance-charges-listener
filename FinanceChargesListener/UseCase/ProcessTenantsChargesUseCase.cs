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
using System.Text.Json;
using System.Threading.Tasks;
using static FinanceChargesListener.Domain.Enums;

namespace FinanceChargesListener.UseCase
{
    public class ProcessTenantsChargesUseCase : IProcessTenantsChargesUseCase
    {
        private readonly ChargesApiGateway _chargesApiGateway;

        public ProcessTenantsChargesUseCase(ChargesApiGateway chargesApiGateway)
        {
            _chargesApiGateway = chargesApiGateway;
        }
        public async Task<bool> ProcessTenantsServiceCharges(List<Asset> assets,
            ChargeType chargeType, EntityMessageSqs headOfChargeData, JsonSerializerOptions jsonSerializerOptions)
        {
            
            if (chargeType == ChargeType.Estate)
            {
                var estateProperties = Utility.Helper.GetAssetsWithEstate(assets);
                
                var chargeAmount = headOfChargeData != null ? Math.Round((headOfChargeData.TotalEstimateAmount / estateProperties.Count), 2) : 0;

                await ApplyTenantsCharges(chargeAmount, headOfChargeData, estateProperties, chargeType);
            }
            else if (chargeType == ChargeType.Block)
            {
                var blockProperties = Utility.Helper.GetAssetsWithBlock(assets);
                var chargeAmount = headOfChargeData != null ? Math.Round((headOfChargeData.TotalEstimateAmount / blockProperties.Count)/52, 2) : 0;

                await ApplyTenantsCharges(chargeAmount, headOfChargeData, blockProperties, chargeType);
            }
            else
            {
                var chargeAmount = headOfChargeData != null ? Math.Round((headOfChargeData.TotalEstimateAmount / assets.Count), 2) : 0;

                await ApplyTenantsCharges(chargeAmount, headOfChargeData, assets, chargeType);
            }
            return true;
        }

        private async Task ApplyTenantsCharges(decimal chargeAmount, EntityMessageSqs entityMessageSqs, List<Asset> assets,
           ChargeType chargeType)
        {
            foreach (var asset in assets)
            {
                var charges = await _chargesApiGateway.GetChargeByTargetIdAsync(asset.Id).ConfigureAwait(false);
               
                // PATCH LOGIC
                if (charges != null && charges.Any())
                {
                    var estimatedTeantCharge = charges.FirstOrDefault(x => x.ChargeGroup == ChargeGroup.Tenants
                           && x.ChargeYear == DateTime.UtcNow.Year);
                    if (estimatedTeantCharge != null && estimatedTeantCharge.DetailedCharges.Any())
                    {
                        foreach (var item in estimatedTeantCharge.DetailedCharges)
                        {
                            if (item.ChargeCode == entityMessageSqs.ChargeCode)
                            {
                                item.ChargeCode = entityMessageSqs.ChargeCode;
                                item.ChargeType = chargeType;
                                item.Amount = chargeAmount;
                            }
                        }
                        if (!estimatedTeantCharge.DetailedCharges.Any(x => x.ChargeCode == entityMessageSqs.ChargeCode))
                        {
                            var chargeDetail = Utility.Helper.GetChargeDetailModel(chargeAmount, entityMessageSqs.ChargeName,
                                entityMessageSqs.ChargeCode, ChargeGroup.Tenants, chargeType);
                            estimatedTeantCharge.DetailedCharges.ToList().Add(chargeDetail);
                        }
                    }
                   
                    await _chargesApiGateway.UpdateChargeAsync(estimatedTeantCharge).ConfigureAwait(false);

                    //var chargeMaintenance = new ChargeMaintenance();
                    //var chargeMaintenanceResponse = await _chargesMaintenanceApiGateway.AddChargeMaintenance(chargeMaintenance).ConfigureAwait(false);
                }
                else
                {
                    var chargeToAdd = Utility.Helper.GetChargeModel(asset.Id, asset.AssetType.ToString(), chargeAmount,
                        entityMessageSqs.ChargeName, entityMessageSqs.ChargeCode, ChargeGroup.Tenants, chargeType);

                    await _chargesApiGateway.AddChargeAsync(chargeToAdd).ConfigureAwait(false);
                }
            }
        }
    }
}
