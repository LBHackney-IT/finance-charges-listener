using FinanceChargesListener.Domain;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface IChargesApiGateway
    {
        Task<Charge> GetChargeByTargetIdAsync(Guid id);
        Task<Charge> AddCharge(AddCharge addCharge);
        Task<Charge> UpdateChargeAsync(Charge charge);
    }
}
