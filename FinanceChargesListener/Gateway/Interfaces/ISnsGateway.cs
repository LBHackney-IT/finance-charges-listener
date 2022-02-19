using FinanceChargesListener.Boundary;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface ISnsGateway
    {
        Task Publish(ChargesSns chargesSns);
    }
}
