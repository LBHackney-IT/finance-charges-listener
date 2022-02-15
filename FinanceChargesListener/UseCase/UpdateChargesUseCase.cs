using FinanceChargesListener.Boundary;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceChargesListener.UseCase
{
    public class UpdateChargesUseCase : IUpdateChargesUseCase
    {
        private readonly IAssetGateway _assetGateway;
        private readonly IChargesGateway _chargesGateway;

        public UpdateChargesUseCase(IAssetGateway assetGateway,
                                    IChargesGateway chargesGateway)
        {
            _assetGateway = assetGateway;
            _chargesGateway = chargesGateway;
        }

        [LogCall]
        public async Task ProcessMessageAsync(ChargesEventSns message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.EventData.NewData is DwellingEventRequest request)
            {
                var asset = await _assetGateway.GetById(request.AssetId);

                if (asset == null)
                {
                    throw new ArgumentNullException(nameof(asset));
                }

                var charges = await _chargesGateway.GetAllByAssetId(asset.Id);

                if (charges == null || charges.Count == 0)
                {
                    throw new ArgumentException($"{nameof(charges)}");
                }

                var chargesDetails = charges.SelectMany(c => c.DetailedCharges);

                foreach (var chargeDetail in request.Details)
                {
                    var updatedDetails = chargesDetails.Where(cd => cd.SubType == chargeDetail.SubType)
                                                       .ToList();

                    updatedDetails.ForEach(cd => cd.Amount = chargeDetail.Amount);
                }

                // update in db
            }

            throw new ArgumentException(nameof(message.EventData.NewData));
        }
    }
}
