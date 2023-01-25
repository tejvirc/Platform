namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Threading;
    using Grpc.Core;

    public class BingoAuthorizationProvider : IBingoAuthorizationProvider, IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new();
        private bool _disposed;
        private Metadata _authorizationData;

        public Metadata AuthorizationData
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _authorizationData;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    _authorizationData = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

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
    }
}