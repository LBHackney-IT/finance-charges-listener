using FinanceChargesListener.Boundary;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static FinanceChargesListener.Domain.Enums;

namespace FinanceChargesListener.UseCase.Interfaces
{
    public interface IProcessLeaseholdChargesUseCase
    {
        Task<bool> ProcessLeaseholderServiceCharges(List<Asset> assets, ChargeType chargeType, EntityMessageSqs entityMessageSqs);
    }
}
