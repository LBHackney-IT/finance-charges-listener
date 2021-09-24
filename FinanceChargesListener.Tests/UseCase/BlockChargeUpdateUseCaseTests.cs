using AutoFixture;
using FinanceChargesListener.Boundary;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Infrastructure.Exceptions;
using FinanceChargesListener.UseCase;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FinanceChargesListener.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class BlockChargeUpdateUseCaseTests
    {
        private readonly Mock<IAssetInformationApiGateway> _mockAssetGateway;
        private readonly Mock<IChargesApiGateway> _mockChargeGateway;
        private readonly Mock<IChargesMaintenanceApiGateway> _mockMaintenanceGateway;
        private readonly BlockChargeUpdateUseCase _sut;
        private readonly Charge _domainEntity;

        private readonly EntityEventSns _message;

        private readonly Fixture _fixture;

        public BlockChargeUpdateUseCaseTests()
        {
            _fixture = new Fixture();

            _mockAssetGateway = new Mock<IAssetInformationApiGateway>();
            _mockChargeGateway = new Mock<IChargesApiGateway>();
            _mockMaintenanceGateway = new Mock<IChargesMaintenanceApiGateway>();

            _sut = new BlockChargeUpdateUseCase(_mockAssetGateway.Object, _mockChargeGateway.Object, _mockMaintenanceGateway.Object);

            _domainEntity = _fixture.Create<Charge>();
            _message = CreateMessage(_domainEntity.Id);

            _mockChargeGateway.Setup(x => x.GetChargeByTargetIdAsync(_domainEntity.Id)).ReturnsAsync(_domainEntity);
        }

        private EntityEventSns CreateMessage(Guid id, string eventType = EventTypes.GlobalChargeUpdatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EntityId, id)
                           .With(x => x.EventType, eventType)
                           .Create();
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestEntityIdNotFoundThrows()
        {
            _mockChargeGateway.Setup(x => x.GetChargeByTargetIdAsync(_domainEntity.Id)).ReturnsAsync((Charge) null);
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<DomainEntity>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestSaveEntityThrows()
        {
            var exMsg = "This is the last error";
            _mockChargeGateway.Setup(x => x.AddCharge(It.IsAny<AddCharge>()))
                        .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

            //_mockChargeGateway.Verify(x => x.AddCharge(_domainEntity.Id), Times.Once);
            //_mockChargeGateway.Verify(x => x.AddCharge(_domainEntity), Times.Once);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestSaveEntitySucceeds()
        {
            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);

            //_mockGateway.Verify(x => x.GetEntityAsync(_domainEntity.Id), Times.Once);
            //_mockGateway.Verify(x => x.SaveEntityAsync(_domainEntity), Times.Once);
        }
    }
}
