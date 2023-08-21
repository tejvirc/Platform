namespace Aristocrat.Monaco.G2S.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Data.SqlClient;
    using System.IO;
    using System.Threading;
    using Kernel;
    using Monaco.Common.Storage;

    /// <summary>
    ///     An implementation of <see cref="IMonacoContextFactory" />
    /// </summary>
    public class DbContextFactory : IMonacoContextFactory, IService, IDisposable
    {
        private readonly string _connectionString;
        private readonly ManualResetEventSlim _exclusiveLock = new(true);

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextFactory" /> class.
        /// </summary>
        public DbContextFactory()
            : this(ServiceManager.GetInstance().GetService<IPathMapper>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextFactory" /> class.
        /// </summary>
        /// <param name="pathMapper">An <see cref="IPathMapper" /> instance.</param>
        public DbContextFactory(IPathMapper pathMapper)
        {
            if (pathMapper == null)
            {
                throw new ArgumentNullException(nameof(pathMapper));
            }

            _connectionString = ConnectionString(pathMapper, Constants.DatabasePassword);
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

        private static string ConnectionString(IPathMapper pathMapper, string password)
        {
            var dir = pathMapper.GetDirectory(Constants.DataPath);
            var path = Path.GetFullPath(dir.FullName);

            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = Path.Combine(path, Constants.DatabaseFileName),
                Password = password
            };

            return sqlBuilder.ConnectionString;
        }

        private DbContext CreateContext()
        {
            var context = new MonacoContext(_connectionString);
            try
            {
                context.Database.EnsureCreated();
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