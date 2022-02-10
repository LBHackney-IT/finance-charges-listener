using AutoFixture;
using FinanceChargesListener.Boundary;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Infrastructure.Exceptions;
using FinanceChargesListener.UseCase;
using FluentAssertions;
using Hackney.Shared.Asset.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FinanceChargesListener.Infrastructure;
using Xunit;

namespace FinanceChargesListener.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class BlockChargeUpdateUseCaseTests
    {
        //private readonly Mock<AssetInformationApiGateway> _mockAssetGateway;
        //private readonly Mock<ChargesApiGateway> _mockChargeGateway;
        //private readonly Mock<ChargesMaintenanceApiGateway> _mockMaintenanceGateway;
        //private readonly BlockChargeUpdateUseCase _sut;
        //private readonly List<Charge> _domainEntity;

        //private readonly EntityEventSns _message;

        //private readonly Fixture _fixture;

        //private readonly JsonSerializerOptions _jsonSerializerOptions = JsonOptions.CreateJsonOptions();

        //public BlockChargeUpdateUseCaseTests()
        //{
        //    _fixture = new Fixture();

        //    _mockAssetGateway = new Mock<AssetInformationApiGateway>();
        //    _mockChargeGateway = new Mock<ChargesApiGateway>();
        //    _mockMaintenanceGateway = new Mock<ChargesMaintenanceApiGateway>();

        //    _sut = new BlockChargeUpdateUseCase(_mockAssetGateway.Object, _mockChargeGateway.Object, _mockMaintenanceGateway.Object);

        //    _domainEntity = _fixture.Create<List<Charge>>();
        //    _message = CreateMessage(_domainEntity[0].Id);

        //    _mockChargeGateway.Setup(x => x.GetChargeByTargetIdAsync(_domainEntity[0].Id)).ReturnsAsync(_domainEntity);
        //}

        //private EntityEventSns CreateMessage(Guid id, string eventType = EventTypes.GlobalChargeUpdatedEvent)
        //{
        //    return _fixture.Build<EntityEventSns>()
        //                   .With(x => x.EntityId, id)
        //                   .With(x => x.EventType, eventType)
        //                   .Create();
        //}

        //[Fact]
        //public void ProcessMessageAsyncTestNullMessageThrows()
        //{ 
        //    Func<Task> func = async () => await _sut.ProcessMessageAsync(null,  _jsonSerializerOptions).ConfigureAwait(false);
        //    func.Should().ThrowAsync<ArgumentNullException>();
        //}

        //[Fact]
        //public void ProcessMessageAsyncTestEntityIdNotFoundThrows()
        //{
        //    _mockChargeGateway.Setup(x => x.GetChargeByTargetIdAsync(_domainEntity[0].Id)).ReturnsAsync((List<Charge>) null);
        //    Func<Task> func = async () => await _sut.ProcessMessageAsync(null, _jsonSerializerOptions).ConfigureAwait(false);
        //    func.Should().ThrowAsync<EntityNotFoundException<DomainEntity>>();
        //}

        //[Fact]
        //public void ProcessMessageAsyncTestSaveEntityThrows()
        //{
        //    var exMsg = "This is the last error";
        //    _mockChargeGateway.Setup(x => x.AddChargeAsync(It.IsAny<Charge>()))
        //                .ThrowsAsync(new Exception(exMsg));

        //    Func<Task> func = async () => await _sut.ProcessMessageAsync(_message, _jsonSerializerOptions).ConfigureAwait(false);
        //    func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

        //    //_mockChargeGateway.Verify(x => x.AddCharge(_domainEntity.Id), Times.Once);
        //    //_mockChargeGateway.Verify(x => x.AddCharge(_domainEntity), Times.Once);
        //}

        //[Fact]
        //public async Task ProcessMessageAsyncTestSaveEntitySucceeds()
        //{
        //    await _sut.ProcessMessageAsync(_message, _jsonSerializerOptions).ConfigureAwait(false);

        //    //_mockGateway.Verify(x => x.GetEntityAsync(_domainEntity.Id), Times.Once);
        //    //_mockGateway.Verify(x => x.SaveEntityAsync(_domainEntity), Times.Once);
        //}
    }
}
