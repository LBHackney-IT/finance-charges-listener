using FinanceChargesListener.Domain;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface IChargesMaintenanceApiGateway
    {
        Task<ChargeMaintenance> AddChargeMaintenance(ChargeMaintenance chargeMaintenance);
    }
}
