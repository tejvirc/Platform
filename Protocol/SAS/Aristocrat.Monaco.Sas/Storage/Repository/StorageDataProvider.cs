namespace Aristocrat.Monaco.Sas.Storage.Repository
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Storage;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     A generic storage data provider for single entity storage in persistence
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class StorageDataProvider<TEntity> : IStorageDataProvider<TEntity>, IDisposable
        where TEntity : BaseEntity, ICloneable, new()
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private bool _disposed;
        private TEntity _entity;

        /// <summary>
        ///     Creates an instance of <see cref="StorageDataProvider{TEntity}" />
        /// </summary>
        /// <param name="unitOfWorkFactory">An instance of <see cref="IUnitOfWorkFactory" /></param>
        public StorageDataProvider(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _entity = LoadFromPersistence();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public TEntity GetData()
        {
            try
            {
                _lock.EnterReadLock();
                return (TEntity)_entity.Clone();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public Task Save(TEntity entity)
        {
            try
            {
                _lock.EnterWriteLock();
                _entity = entity;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return Task.Run(() => UpdateDatabase(entity));
        }

        /// <inheritdoc />
        public void Save(TEntity entity, IUnitOfWork work)
        {
            try
            {
                _lock.EnterWriteLock();
                _entity = entity;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            var repository = work.Repository<TEntity>();
            repository.Update(entity);
        }

        /// <summary>
        ///     Disposes the resources used by this instance
        /// </summary>
        /// <param name="disposing">Whether or not you are disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _lock.Dispose();
            }

            _disposed = true;
        }

        private TEntity LoadFromPersistence()
        {
            var entity = _unitOfWorkFactory.Invoke(x => x.Repository<TEntity>().Queryable().SingleOrDefault());
            if (entity is null)
            {
                entity = new TEntity();
                UpdateDatabase(entity, true);
            }

            return entity;
        }

        private void UpdateDatabase(TEntity entity, bool add = false)
        {
            using var work = _unitOfWorkFactory.Create();
            work.BeginTransaction(IsolationLevel.Serializable);
            var repository = work.Repository<TEntity>();
            if (add)
            {
                repository.Add(entity);
            }
            else
            {
                repository.Update(entity);
            }

            work.Commit();
        }
    }
}