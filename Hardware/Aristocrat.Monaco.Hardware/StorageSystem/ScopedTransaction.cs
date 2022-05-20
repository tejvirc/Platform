namespace Aristocrat.Monaco.Hardware.StorageSystem
{
    using System;
    using Contracts.Persistence;

    /// <summary>
    ///     Defines a mechanism that can be used to wrap multiple writes against persistent storage
    /// </summary>
    public class ScopedTransaction : IScopedTransaction
    {
        [ThreadStatic] private static bool _active;

        private readonly object _lock = new object();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScopedTransaction" /> class.
        /// </summary>
        /// <param name="connectionString">Block's connection string.</param>
        public ScopedTransaction(string connectionString)
        {
            PersistenceTransaction.Ready = false;
            PersistenceTransaction.Current = new SqlPersistentStorageTransaction(connectionString);

            _active = true;
        }

        /// <summary>
        ///     Completes the current transaction
        /// </summary>
        public void Complete()
        {
            lock (_lock)
            {
                if (!_active || PersistenceTransaction.Current == null)
                {
                    return;
                }

                _active = false;
                PersistenceTransaction.Ready = true;
                PersistenceTransaction.Current.Commit();
                PersistenceTransaction.Current = null;
                PersistenceTransaction.Ready = false;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                lock (_lock)
                {
                    _active = false;

                    PersistenceTransaction.Ready = true;
                    PersistenceTransaction.Current?.Rollback();
                }
            }

            PersistenceTransaction.Current = null;
            PersistenceTransaction.Ready = false;

            _disposed = true;
        }
    }
}