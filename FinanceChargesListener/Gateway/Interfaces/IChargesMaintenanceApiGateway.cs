using FinanceChargesListener.Domain;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface ChargesMaintenanceApiGateway
    {
        Task<ChargeMaintenance> AddChargeMaintenance(ChargeMaintenance chargeMaintenance);
    }
}
