using FinanceChargesListener.Boundary;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static FinanceChargesListener.Domain.Enums;

namespace FinanceChargesListener.UseCase.Interfaces
{
    public interface IProcessTenantsChargesUseCase
    {
        Task<bool> ProcessTenantsServiceCharges(List<Asset> assets, ChargeType chargeType,
             EntityMessageSqs headOfChargeData, JsonSerializerOptions jsonSerializerOptions);
    }
}
