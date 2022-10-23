namespace Aristocrat.Monaco.Protocol.Common.Storage.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Monaco.Common.Storage;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    /// <summary>
    ///     Encapsulates a set of operations performed on the database.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Container _container;

        private readonly Dictionary<Type, InstanceProducer> _repositories = new Dictionary<Type, InstanceProducer>();

        private Scope _scope;
        private DbContext _context;
        private IDbContextTransaction _transaction;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="container">Dependency injection container.</param>
        public UnitOfWork(Container container)
        {
            _container = container;

            _scope = AsyncScopedLifestyle.BeginScope(_container);

            _context = _scope.GetInstance<DbContext>();
        }

        /// <inheritdoc />
        ~UnitOfWork()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public Repositories.IRepository<TEntity> Repository<TEntity>()
            where TEntity : BaseEntity
        {
            var entityType = typeof(TEntity);

            if (!_repositories.TryGetValue(entityType, out var producer))
            {
                producer = _container.GetRegistration(typeof(Repositories.IRepository<>).MakeGenericType(entityType));

                _repositories.Add(entityType, producer);
            }

            if (producer == null)
            {
                throw new InvalidOperationException($"No repository for {entityType.Name} entity type");
            }

            return (Repositories.IRepository<TEntity>)_scope.GetInstance(producer.ServiceType);
        }

        /// <inheritdoc />
        public int? CommandTimeout
        {
            get => _context.Database.GetCommandTimeout();

            set => _context.Database.SetCommandTimeout(value);
        }

        /// <inheritdoc />
        public int SaveChanges() => _context.SaveChanges();

        /// <inheritdoc />
        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (_context.Database.GetDbConnection().State != ConnectionState.Open)
            {
                _context.Database.GetDbConnection().Open();
            }

            _transaction = _context.Database.BeginTransaction(isolationLevel);
        }

        /// <inheritdoc />
        public void Commit()
        {
            try
            {
                _transaction?.Commit();
            }
            finally
            {
                _transaction = null;
            }
        }

        /// <inheritdoc />
        public void Rollback()
        {
            try
            {
                _transaction?.Rollback();
            }
            finally
            {
                _transaction = null;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // ReSharper disable once UseNullPropagation
                if (_transaction != null)
                {
                    _transaction.Dispose();
                }

                // ReSharper disable once UseNullPropagation
                if (_scope != null)
                {
                    _scope.Dispose();
                }
            }

            _transaction = null;
            _scope = null;
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }

            _disposed = true;
        }
    }
}
