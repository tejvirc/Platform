namespace Aristocrat.Monaco.G2S.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.EntityFrameworkCore;
    using Kernel;
    using Monaco.Common.Storage;
    using Protocol.Common.Storage;
    using G2S.Common.Data;

    /// <summary>
    ///     An implementation of <see cref="IMonacoContextFactory" />
    /// </summary>
    public class DbContextFactory : IMonacoContextFactory, IService, IDisposable
    {
        private readonly IConnectionStringResolver _connectionStringResolver;
        private readonly ManualResetEventSlim _exclusiveLock = new(true);

        private bool _disposed;

        public DbContextFactory()
            : this(new DefaultConnectionStringResolver(ServiceManager.GetInstance().GetService<IPathMapper>()))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextFactory" /> class.
        /// </summary>
        public DbContextFactory(IConnectionStringResolver connectionStringResolver)
        {
            _connectionStringResolver = connectionStringResolver ??
                                        throw new ArgumentNullException(nameof(connectionStringResolver));
            using var context = new MonacoContext(_connectionStringResolver);
            context.Database.EnsureCreated();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public DbContext CreateDbContext()
        {
            _exclusiveLock.Wait();
            return CreateContext();
        }

        /// <inheritdoc />
        public DbContext Lock()
        {
            _exclusiveLock.Reset();
            return CreateContext();
        }

        /// <inheritdoc />
        public void Release()
        {
            _exclusiveLock.Set();
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IMonacoContextFactory) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _exclusiveLock.Dispose();
            }

            _disposed = true;
        }

        private DbContext CreateContext()
        {
            var context = new MonacoContext(_connectionStringResolver);
            try
            {
                return context;
            }
            catch
            {
                context.Dispose();
                throw;
            }
        }
    }
}