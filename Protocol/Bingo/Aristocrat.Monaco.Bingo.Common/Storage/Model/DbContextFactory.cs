namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Threading;
    using Kernel;
    using Monaco.Common.Storage;
    using Protocol.Common.Storage;

    public class DbContextFactory : IMonacoContextFactory, IService, IDisposable
    {
        private readonly IConnectionStringResolver _connectionStringResolver;
        private readonly ManualResetEventSlim _exclusiveLock = new(true);

        private bool _disposed;

        public DbContextFactory()
            : this(new DefaultConnectionStringResolver(ServiceManager.GetInstance().GetService<IPathMapper>()))
        {
        }

        public DbContextFactory(IConnectionStringResolver connectionStringResolver)
        {
            _connectionStringResolver = connectionStringResolver ??
                                        throw new ArgumentNullException(nameof(connectionStringResolver));
            using var context = new BingoContext(_connectionStringResolver);
            context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public DbContext CreateDbContext()
        {
            _exclusiveLock.Wait();
            return CreateContext();
        }

        public DbContext Lock()
        {
            _exclusiveLock.Reset();
            return CreateContext();
        }

        public void Release()
        {
            _exclusiveLock.Set();
        }

        public string Name => GetType().Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(IMonacoContextFactory) };

        public void Initialize()
        {
        }

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
            var context = new BingoContext(_connectionStringResolver);
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