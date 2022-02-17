using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase.Interfaces
{
    public interface IManagementFeeUseCase
    {
        Task<bool> ApplyManagementFee(List<Asset> assets);
    }
}
