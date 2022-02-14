using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using FinanceChargesListener.Infrastructure;
using FinanceChargesListener.Infrastructure.Entity;
using System;

namespace FinanceChargesListener.Tests.E2ETests.Fixtures
{
    public class ChargesFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly IDynamoDBContext _dbContext;

        public ChargeDbEntity DbEntity { get; private set; }
        public Guid DbEntityId { get; private set; }

        public ChargesFixture(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
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
                if (null != DbEntity)
                    _dbContext.DeleteAsync<DbEntity>(DbEntity.Id).GetAwaiter().GetResult();

                _disposed = true;
            }
        }

        private ChargeDbEntity ConstructAndSaveEntity(Guid id, Guid? targetId = null)
        {
            var dbEntity = _fixture.Build<ChargeDbEntity>()
                                 .With(x => x.Id, id)
                                 .With(x => x.TargetId, targetId)
                                 .Create();

            _dbContext.SaveAsync<ChargeDbEntity>(dbEntity).GetAwaiter().GetResult();
            dbEntity.TargetId = Guid.Empty;
            return dbEntity;
        }

        public void GivenAnEntityAlreadyExists(Guid id)
        {
            if (null == DbEntity)
            {
                var entity = ConstructAndSaveEntity(id);
                DbEntity = entity;
                DbEntityId = entity.Id;
            }
        }

        public void GivenAnEntityDoesNotExist(Guid id)
        {
            // Nothing to do here
        }
    }
}
