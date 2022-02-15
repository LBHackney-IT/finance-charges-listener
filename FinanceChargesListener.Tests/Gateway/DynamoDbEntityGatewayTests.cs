using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Factories;
using FinanceChargesListener.Gateway;
using FinanceChargesListener.Infrastructure;
using FinanceChargesListener.Infrastructure.Entity;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FinanceChargesListener.Tests.Gateway
{
    [Collection("Aws collection")]
    public class DynamoDbEntityGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ILogger<ChargesApiGateway>> _logger;
        private readonly ChargesApiGateway _classUnderTest;
        private AwsIntegrationTest _dbTestFixture;
        private IAmazonDynamoDB AmazonDynamoDb => _dbTestFixture.DynamoDb;
        private IDynamoDBContext DynamoDb => _dbTestFixture.DynamoDbContext;
        private readonly List<Action> _cleanup = new List<Action>();


        public DynamoDbEntityGatewayTests(AwsIntegrationTest dbTestFixture)
        {
            _dbTestFixture = dbTestFixture;
            _logger = new Mock<ILogger<ChargesApiGateway>>();
            _classUnderTest = new ChargesApiGateway(DynamoDb, AmazonDynamoDb, _logger.Object);
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
                foreach (var action in _cleanup)
                    action();

                if (_dbTestFixture != null)
                {
                    _dbTestFixture.Dispose();
                    _dbTestFixture = null;
                }

                _disposed = true;
            }
        }

        private async Task InsertDatatoDynamoDB(Charge entity)
        {
            await DynamoDb.SaveAsync(entity.ToDatabase()).ConfigureAwait(false);
            _cleanup.Add(async () => await DynamoDb.DeleteAsync<ChargeDbEntity>(entity.Id).ConfigureAwait(false));
        }

        private Charge ConstructDomainEntity()
        {
            var entity = _fixture.Build<Charge>()
                                 .Create();
            return entity;
        }

        //[Fact]
        //public async Task GetEntityAsyncTestReturnsRecord()
        //{
        //    var domainEntity = ConstructDomainEntity();
        //    await InsertDatatoDynamoDB(domainEntity).ConfigureAwait(false);

        //    var result = await _classUnderTest.GetChargeByTargetIdAsync(domainEntity.TargetId).ConfigureAwait(false);

        //    //result.Should().BeEquivalentTo(domainEntity, e => e.Excluding(y => y.));
        //    //result.VersionNumber.Should().Be(0);

        //    _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.LoadAsync for id {domainEntity.Id}", Times.Once());
        //}

        //[Fact]
        //public async Task GetEntityAsyncTestReturnsNullWhenNotFound()
        //{
        //    var id = Guid.NewGuid();
        //    var result = await _classUnderTest.GetChargeByTargetIdAsync(id).ConfigureAwait(false);

        //    result.Should().BeNull();

        //    _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.LoadAsync for id {id}", Times.Once());
        //}

        //[Fact]
        //public async Task SaveEntityAsyncTestUpdatesDatabase()
        //{
        //    var domainEntity = ConstructDomainEntity();
        //    await InsertDatatoDynamoDB(domainEntity).ConfigureAwait(false);

        //    domainEntity.Id = Guid.NewGuid();
        //    domainEntity.TargetId = Guid.NewGuid();
        //    await _classUnderTest.AddChargeAsync(domainEntity).ConfigureAwait(false);

        //    var updatedInDb = await DynamoDb.LoadAsync<ChargeDbEntity>(domainEntity.Id).ConfigureAwait(false);
        //    updatedInDb.ToDomain().Should().BeEquivalentTo(domainEntity, (e) => e.Excluding(y => y.CreatedBy)
        //                                                                         .Excluding(y => y.CreatedAt)
        //                                                                         .Excluding(y => y.LastUpdatedAt)
        //                                                                         .Excluding(y => y.LastUpdatedBy));
        //    updatedInDb.TargetId.Should().Be(domainEntity.TargetId);

        //    _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.SaveAsync for id {domainEntity.Id}", Times.Once());
        //}
    }
}
