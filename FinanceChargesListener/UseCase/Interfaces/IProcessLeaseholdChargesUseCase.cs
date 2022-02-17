using FinanceChargesListener.Boundary;
using FinanceChargesListener.Domain;
using Hackney.Shared.Asset.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase.Interfaces
{
    public interface IProcessLeaseholdChargesUseCase
    {
        Task<bool> ProcessLeaseholderServiceCharges(List<Asset> assets, ChargeType chargeType, EntityMessageSqs entityMessageSqs);
    }
}
