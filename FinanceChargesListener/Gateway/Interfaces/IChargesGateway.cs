using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface IChargesGateway
    {
        Task<Charge> GetById(Guid chargeId, Guid assetId);
        Task<List<Charge>> GetAllByAssetId(Guid assetId);
        Task<bool> SaveBatchAsync(List<Charge> charges);
    }
}
