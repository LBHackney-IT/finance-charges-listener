using FinanceChargesListener.Boundary;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase.Interfaces
{
    public interface MessageProcessing
    {
        Task ProcessMessageAsync(EntityEventSns message, JsonSerializerOptions jsonSerializerOptions);
    }
}
