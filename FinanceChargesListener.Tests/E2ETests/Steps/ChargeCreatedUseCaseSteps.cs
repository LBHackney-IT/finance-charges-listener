using Amazon.DynamoDBv2.DataModel;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Infrastructure;
using FinanceChargesListener.Infrastructure.Entity;
using FinanceChargesListener.Infrastructure.Exceptions;
using FluentAssertions;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Tests.E2ETests.Steps
{
    public class ChargeCreatedUseCaseSteps : BaseSteps
    {
        public ChargeCreatedUseCaseSteps()
        { }

        public async Task WhenTheFunctionIsTriggered(Guid id)
        {
            await TriggerFunction(id).ConfigureAwait(false);
        }

        public async Task ThenTheEntityIsUpdated(ChargeDbEntity beforeChange, IDynamoDBContext dbContext)
        {
            var entityInDb = await dbContext.LoadAsync<ChargeDbEntity>(beforeChange.Id);

            entityInDb.Should().BeEquivalentTo(beforeChange,
                config => config.Excluding(y => y.CreatedAt)
                                .Excluding(z => z.LastUpdatedAt));
            entityInDb.ChargeSubGroup.Should().Be("Estimate");

        }

        public void ThenAnEntityNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<Charge>));
            (_lastException as EntityNotFoundException<Charge>).Id.Should().Be(id);
        }
    }
}
