namespace Aristocrat.Monaco.Kernel.LockManagement
{
    using System;
    using System.Threading;

    /// <summary>
    ///     Adding extension methods to ReaderWriterLockSlim to return IDisposable token that release lock taken on dispose
    /// </summary>
    public static class ReaderWriterLockSlimExtension
    {
        /// <summary>
        ///     Get read lock on ReaderWriterLockSlim
        /// </summary>
        /// <param name="obj">ReaderWriterLockSlim to lock</param>
        /// <returns>an IDisposable token which releases lock on dispose</returns>
        public static IDisposable GetReadLock(this ReaderWriterLockSlim obj)
        {
            return new LockToken(obj, true);
        }

        /// <summary>
        ///     TryEnterRead lock with timeout
        /// </summary>
        /// <param name="obj">ReaderWriterLockSlim to lock</param>
        /// <param name="timeout">timeout in milliseconds</param>
        /// <param name="token">an IDisposable token which releases lock on dispose</param>
        /// <returns>if lock was taken</returns>
        public static bool TryGetReadLock(this ReaderWriterLockSlim obj, int timeout, out IDisposable token)
        {
            return ReturnTokenLockTaken(obj, true, timeout, out token);
        }

        /// <summary>
        ///     Get write lock on ReaderWriterLockSlim
        /// </summary>
        /// <param name="obj">ReaderWriterLockSlim to lock</param>
        /// <returns>an IDisposable token which releases lock on dispose</returns>
        public static IDisposable GetWriteLock(this ReaderWriterLockSlim obj)
        {
            return new LockToken(obj, false);
        }

        /// <summary>
        ///     TryEnterWrite lock with timeout
        /// </summary>
        /// <param name="obj">ReaderWriterLockSlim to lock</param>
        /// <param name="timeout">timeout in milliseconds</param>
        /// <param name="token">an IDisposable token which releases lock on dispose</param>
        /// <returns>if lock was taken</returns>
        public static bool TryGetWriteLock(this ReaderWriterLockSlim obj, int timeout, out IDisposable token)
        {
            return ReturnTokenLockTaken(obj, false, timeout, out token);
        }

        /// <summary>
        ///     Checks the lock type taken and release the lock
        /// </summary>
        /// <param name="obj">ReaderWriterLockSlim to release the lock</param>
        public static void ReleaseLock(this ReaderWriterLockSlim obj)
        {
            if (obj.IsWriteLockHeld)
            {
                obj.ExitWriteLock();
            }
            else if (obj.IsUpgradeableReadLockHeld)
            {
                obj.ExitUpgradeableReadLock();
            }
            else if (obj.IsReadLockHeld)
            {
                obj.ExitReadLock();
            }
        }

        private static bool ReturnTokenLockTaken(
            ReaderWriterLockSlim obj,
            bool isReadOnlyLock,
            int timeout,
            out IDisposable token)
        {
            var returnToken = new LockToken(obj, isReadOnlyLock, timeout);
            token = returnToken.LockTaken ? returnToken : null;
            return returnToken.LockTaken;
        }

        private enum LockTokenType
        {
            None,
            ReadOnly,
            Write
        }

        private sealed class LockToken : IDisposable
        {
            private ReaderWriterLockSlim _sync;
            private LockTokenType _tokenType;

            public LockToken(ReaderWriterLockSlim sync, bool isReadOnlyLock, int timeout = -1)
            {
                _sync = sync;
                if (isReadOnlyLock)
                {
                    if (_sync.IsReadLockHeld || _sync.IsWriteLockHeld || _sync.IsUpgradeableReadLockHeld)
                    {
                        _tokenType = LockTokenType.None;
                        LockTaken = true;
                    }
                    else
                    {
                        EnterReadOnlyLock(timeout);
                    }
                }
                else
                {
                    if (_sync.IsWriteLockHeld)
                    {
                        _tokenType = LockTokenType.None;
                        LockTaken = true;
                    }
                    else if (_sync.IsUpgradeableReadLockHeld)
                    {
                        EnterWriteLock(timeout);
                    }
                    else if (_sync.IsReadLockHeld)
                    {
                        throw new LockRecursionException(
                            "Current thread already have a read only lock on the ReaderWriterLockSlim, it can not enter into the write lock.");
                    }
                    else
                    {
                        EnterWriteLock(timeout);
                    }
                }
            }

            public bool LockTaken { get; private set; }

            public void Dispose()
            {
                if (_sync == null)
                {
                    return;
                }

                if (LockTaken)
                {
                    if (_tokenType == LockTokenType.ReadOnly)
                    {
                        _sync.ExitReadLock();
                    }
                    else if (_tokenType == LockTokenType.Write)
                    {
                        _sync.ExitWriteLock();
                    }
                }

                _sync = null;
            }

            private void EnterReadOnlyLock(int timeout)
            {
                LockTaken = _sync.TryEnterReadLock(timeout);
                _tokenType = LockTokenType.ReadOnly;
            }

            private void EnterWriteLock(int timeout)
            {
                LockTaken = _sync.TryEnterWriteLock(timeout);
                _tokenType = LockTokenType.Write;
            }
        }
    }
}