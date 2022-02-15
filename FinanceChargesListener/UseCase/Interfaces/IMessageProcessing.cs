using FinanceChargesListener.Boundary;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase.Interfaces
{
    public interface IMessageProcessing
    {
        Task ProcessMessageAsync(ChargesEventSns message);
    }
}
