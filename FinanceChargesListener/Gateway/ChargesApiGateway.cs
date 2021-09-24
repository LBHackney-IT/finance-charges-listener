using FinanceChargesListener.Domain;
using FinanceChargesListener.Gateway.Interfaces;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class ChargesApiGateway : IChargesApiGateway
    {
        public Task<Charge> AddCharge(AddCharge addCharge)
        {
            throw new NotImplementedException();
        }

        public Task<Charge> GetChargeByTargetIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Charge> UpdateChargeAsync(Charge charge)
        {
            throw new NotImplementedException();
        }
    }
}
