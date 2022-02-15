using FinanceChargesListener.Tests.E2ETests.Fixtures;
using FinanceChargesListener.Tests.E2ETests.Steps;
using System;
using TestStack.BDDfy;
using Xunit;

namespace FinanceChargesListener.Tests.E2ETests.Stories
{
    [Story(
        AsA = "SQS Entity Listener",
        IWant = "a function to process the DoSomething message",
        SoThat = "The correct details are set on the entity")]
    [Collection("DynamoDb collection")]
    public class DoSomethingTests : IDisposable
    {
        private readonly AwsIntegrationTest _dbFixture;
        private readonly ChargesFixture _entityFixture;

        private readonly ChargeCreatedUseCaseSteps _steps;

        public DoSomethingTests(AwsIntegrationTest dbFixture)
        {
            _dbFixture = dbFixture;

            _entityFixture = new ChargesFixture(_dbFixture.DynamoDbContext);

            _steps = new ChargeCreatedUseCaseSteps();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _entityFixture.Dispose();

                _disposed = true;
            }
        }

        //[Fact]
        //public void ListenerUpdatesTheEntity()
        //{
        //    var id = Guid.NewGuid();
        //    this.Given(g => _entityFixture.GivenAnEntityAlreadyExists(id))
        //        .When(w => _steps.WhenTheFunctionIsTriggered(id))
        //        .Then(t => _steps.ThenTheEntityIsUpdated(_entityFixture.DbEntity, _dbFixture.DynamoDbContext))
        //        .BDDfy();
        //}

        //[Fact]
        //public void EntityNotFound()
        //{
        //    var id = Guid.NewGuid();
        //    this.Given(g => _entityFixture.GivenAnEntityDoesNotExist(id))
        //        .When(w => _steps.WhenTheFunctionIsTriggered(id))
        //        .Then(t => _steps.ThenAnEntityNotFoundExceptionIsThrown(id))
        //        .BDDfy();
        //}
    }
}
