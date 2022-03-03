using FinanceChargesListener.UseCase.Interfaces;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase
{
    public class ManagementFeeUseCase : IManagementFeeUseCase
    {
        public Task<bool> ApplyManagementFee(List<Asset> assets)
        {
            throw new NotImplementedException();
        }
    }
}
