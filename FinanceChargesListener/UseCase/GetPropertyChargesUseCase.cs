using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FinanceChargesListener.Boundary;
using FinanceChargesListener.Boundary.Response;
using FinanceChargesListener.Factories;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.UseCase.Interfaces;

namespace FinanceChargesListener.UseCase
{
    public class GetPropertyChargesUseCase : IGetPropertyChargesUseCase
    {
        private readonly IChargesApiGateway _gateway;

        public GetPropertyChargesUseCase(IChargesApiGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<List<ChargeResponse>> ExecuteAsync(PropertyChargesMessageSqs queryParameters)
        {
            var result = await _gateway.GetChargesByYearGroupSubGroup(queryParameters.ChargeYear,queryParameters.ChargeGroup,queryParameters.ChargeSubGroup).ConfigureAwait(false);

            return result.ToResponse();
        }
    }
}
