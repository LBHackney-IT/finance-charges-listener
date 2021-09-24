using FinanceChargesListener.Domain;
using FinanceChargesListener.Gateway.Interfaces;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    public class ChargesMaintenanceApiGateway : IChargesMaintenanceApiGateway
    {
        public Task<ChargeMaintenance> AddChargeMaintenance(ChargeMaintenance chargeMaintenance)
        {
            throw new NotImplementedException();
        }
    }
}
