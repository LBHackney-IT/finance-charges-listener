using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinanceChargesListener.Boundary;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface IChargesApiGateway
    {
        Task<List<Charge>> GetChargeByTargetIdAsync(Guid id);
        Task AddChargeAsync(Charge charge);
        Task UpdateChargeAsync(Charge charge);
        public Task<bool> AddTransactionBatchAsync(List<Charge> charges);
        Task<bool> SaveBatchAsync(List<Charge> charges);
        Task<Charge> GetById(Guid chargeId, Guid assetId);
        Task DeleteBatchAsync(List<ChargeKeys> chargeIds, int batchCapacity);
        Task<List<Charge>> GetChargesByYearGroupSubGroup(short chargeYear, ChargeGroup chargeGroup, ChargeSubGroup? chargeSubGroup);
    }
}
