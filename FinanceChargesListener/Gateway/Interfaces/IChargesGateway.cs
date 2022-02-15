using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface IChargesGateway
    {
        Task<List<Charge>> GetAllByAssetId(Guid assetId);
    }
}
