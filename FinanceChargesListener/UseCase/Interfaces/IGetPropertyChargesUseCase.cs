using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FinanceChargesListener.Boundary;
using FinanceChargesListener.Boundary.Response;

namespace FinanceChargesListener.UseCase.Interfaces
{
    public interface IGetPropertyChargesUseCase
    {
        Task<List<ChargeResponse>> ExecuteAsync(PropertyChargesMessageSqs queryParameters);
    }
}
