namespace Aristocrat.Monaco.Kernel.LockManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts.LockManagement;
    using log4net;

    /// <summary>
    ///     SimpleLockManager is a non-complex implementation of ILockManager
    ///     for managing synchronization of ILockable entities
    /// </summary>
    public class SimpleLockManager : ILockManager, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan ThresholdWaitTime = TimeSpan.FromMilliseconds(100d);

        /// <inheritdoc />
        public IDisposable AcquireReadOnlyLock(IEnumerable<ILockable> resources)
        {
            var resourceList = resources.ToList();
            while (true)
            {
                if (TryAcquireLock(resourceList, ThresholdWaitTime.Milliseconds, out var tokens, true))
                {
                    return tokens;
                }
            }
        }

        /// <inheritdoc />
        public IDisposable AcquireExclusiveLock(IEnumerable<ILockable> resources)
        {
            var resourceList = resources.ToList();
            while (true)
            {
                if (TryAcquireLock(resourceList, ThresholdWaitTime.Milliseconds, out var tokens, false))
                {
                    return tokens;
                }
            }
        }

        /// <inheritdoc />
        public bool TryAcquireReadOnlyLock(IEnumerable<ILockable> resources, int timeout, out IDisposable token)
        {
            return TryAcquireLock(resources, timeout, out token, true);
        }

        /// <inheritdoc />
        public bool TryAcquireExclusiveLock(IEnumerable<ILockable> resources, int timeout, out IDisposable token)
        {
            return TryAcquireLock(resources, timeout, out token, false);
        }

        /// <inheritdoc />
        public void ReleaseLock(IEnumerable<ILockable> resources)
        {
            foreach (var resource in resources)
            {
                resource.ReleaseLock();
            }
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ILockManager) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Simple lock manager is initialized.");
        }

        private bool TryAcquireLock(
            IEnumerable<ILockable> resources,
            int timeout,
            out IDisposable token,
            bool isReadOnly)
        {
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            token = new DisposableCollection<IDisposable>();
            try
            {
                var orderedResources = resources.OrderBy(r => r.UniqueLockableName);
                foreach (var r in orderedResources)
                {
                    if (r == null)
                    {
                        throw new NullReferenceException(nameof(r));
                    }

                    var entryTimeStamp = DateTime.UtcNow;
                    IDisposable resourceToken;
                    if (isReadOnly && r is IReadWriteLockable rw
                        ? rw.TryAcquireReadOnlyLock(timeout, out resourceToken)
                        : r.TryAcquireExclusiveLock(timeout, out resourceToken))
                    {
                        ((DisposableCollection<IDisposable>)token).Add(resourceToken);

                        var lockTakenTimeStamp = DateTime.UtcNow;
                        var timeTaken = lockTakenTimeStamp.Subtract(entryTimeStamp);
                        if (timeTaken > ThresholdWaitTime)
                        {
                            Logger.Warn(
                                $"Lock manager took more then threshold time to acquire lock. Time taken is {timeTaken.TotalMilliseconds} milliseconds.");
                        }
                    }
                    else
                    {
                        Logger.Info(
                            $"Lock manager failed to acquire lock on {r.UniqueLockableName} and timeout({timeout} milliseconds) occurred.");
                        token.Dispose();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Exception'{ex.Message}' occurred in lock manager while trying to take lock on resources.",
                    ex);
                token.Dispose();
                throw;
            }

            return true;
        }
    }
}