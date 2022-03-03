using FinanceChargesListener.Boundary;
using FinanceChargesListener.Domain;
using Hackney.Shared.Asset.Domain;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase.Interfaces
{
    public interface IProcessTenantsChargesUseCase
    {
        Task<bool> ProcessTenantsServiceCharges(List<Asset> assets, ChargeType chargeType,
             EntityMessageSqs headOfChargeData, JsonSerializerOptions jsonSerializerOptions);
    }
}
