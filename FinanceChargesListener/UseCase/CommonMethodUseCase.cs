using FinanceChargesListener.Boundary;
using FinanceChargesListener.UseCase.Interfaces;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase
{
    public class CommonMethodUseCase : ICommonMethodUseCase
    {
        private readonly IProcessTenantsChargesUseCase _processTenantsChargesUseCase;
        private readonly IProcessLeaseholdChargesUseCase _processLeaseholdChargesUseCase;

        public CommonMethodUseCase(IProcessTenantsChargesUseCase processTenantsChargesUseCase,
            IProcessLeaseholdChargesUseCase processLeaseholdChargesUseCase)
        {
            _processTenantsChargesUseCase = processTenantsChargesUseCase;
            _processLeaseholdChargesUseCase = processLeaseholdChargesUseCase;
        }
        public async Task<bool> ApportionCharge(List<Asset> assets,  EntityEventSns message,
            JsonSerializerOptions jsonSerializerOptions)
        {
            var headOfChargeData = JsonSerializer.Deserialize<EntityMessageSqs>(message?.EventData?.NewData?.ToString()
                ?? string.Empty, jsonSerializerOptions);

            var chargeType = Utility.Helper.GetChargeType(headOfChargeData.ChargeName);

            var isTenantsApplicable = Utility.Helper.IsChargeApplicableForTenants(headOfChargeData.ChargeName);

            if (isTenantsApplicable)
            {
                await _processTenantsChargesUseCase.ProcessTenantsServiceCharges(assets, chargeType, headOfChargeData, jsonSerializerOptions)
                    .ConfigureAwait(false);
            }
            await _processLeaseholdChargesUseCase.ProcessLeaseholderServiceCharges(assets, chargeType, headOfChargeData)
                .ConfigureAwait(false);

            return true;
        }
    }
}
