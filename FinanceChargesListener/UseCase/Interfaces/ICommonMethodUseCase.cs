using FinanceChargesListener.Boundary;
using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase.Interfaces
{
    public interface ICommonMethodUseCase
    {
        Task<bool> ApportionCharge(List<Asset> assets, EntityEventSns message, JsonSerializerOptions jsonSerializerOptions);
    }
}
