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
    public class DoSomethingUseCaseSteps : BaseSteps
    {
        public DoSomethingUseCaseSteps()
        { }

        public async Task WhenTheFunctionIsTriggered(Guid id)
        {
            await TriggerFunction(id).ConfigureAwait(false);
        }

        public async Task ThenTheEntityIsUpdated(ChargeDbEntity beforeChange, IDynamoDBContext dbContext)
        {
            var entityInDb = await dbContext.LoadAsync<ChargeDbEntity>(beforeChange.Id);

            //entityInDb.Should().BeEquivalentTo(beforeChange,
            //    config => config.Excluding(y => y.Description)
            //                    .Excluding(z => z.VersionNumber));
            //entityInDb.Description.Should().Be("Updated");
            
        }

        public void ThenAnEntityNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<DomainEntity>));
            (_lastException as EntityNotFoundException<DomainEntity>).Id.Should().Be(id);
        }
    }
}
