using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface ChargesApiGateway
    {
        Task<List<Charge>> GetChargeByTargetIdAsync(Guid id);
        Task AddChargeAsync(Charge charge);
        Task UpdateChargeAsync(Charge charge);
        public Task<bool> AddTransactionBatchAsync(List<Charge> charges);
        Task<bool> SaveBatchAsync(List<Charge> charges);
        Task<Charge> GetById(Guid chargeId, Guid assetId);
    }
}
