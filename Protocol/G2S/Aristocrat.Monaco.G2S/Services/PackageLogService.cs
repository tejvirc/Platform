namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Client;
    using Data.Model;
    using Data.Packages;
    using Monaco.Common.Storage;

    /// <summary>
    ///     An implementation of <see cref="IPackageLog" />
    /// </summary>
    public class PackageLogService : IPackageLog, IDisposable
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IPackageLogRepository _packageLogs;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageLogService" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="packageLogs">An <see cref="IPackageLogRepository" /> instance.</param>
        public PackageLogService(
            IMonacoContextFactory contextFactory,
            IPackageLogRepository packageLogs)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _packageLogs = packageLogs ?? throw new ArgumentNullException(nameof(packageLogs));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public int MinimumLogEntries => Constants.DefaultMinLogEntries;

        /// <inheritdoc />
        public int Entries
        {
            get
            {
                using (var context = _contextFactory.Create())
                {
                    return _packageLogs.Count(context);
                }
            }
        }

        /// <inheritdoc />
        public long LastSequence
        {
            get
            {
                using (var context = _contextFactory.Create())
                {
                    return _packageLogs.GetAll(context).Max(x => (long?)x.Id) ?? 0;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<PackageLog> GetLogs()
        {
            using (var context = _contextFactory.Create())
            {
                return _packageLogs.GetAll(context).ToList();
            }
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
            
            _disposed = true;
        }
    }
}