namespace Aristocrat.Monaco.Kernel.Contracts.LockManagement
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Implementation of ILockManager will be responsible for managing synchronization of ILockable entities
    /// </summary>
    public interface ILockManager
    {
        /// <summary>
        ///     Acquire read only lock on resources
        /// </summary>
        /// <param name="resources">resources to lock</param>
        /// <returns>IDisposable token to release locks on all the resources</returns>
        IDisposable AcquireReadOnlyLock(IEnumerable<ILockable> resources);

        /// <summary>
        ///     Try to acquire read only lock on all resources and fail if timeout occurs
        /// </summary>
        /// <param name="resources">resources to lock</param>
        /// <param name="timeout">timeout in milliseconds for each resource. -1 is reserved for indefinite wait.</param>
        /// <param name="token">IDisposable token to release locks on all the resources</param>
        /// <returns>If lock is taken</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when timeout is negative, but it is not equal to Infinite (-1),
        ///     which is the only negative value allowed.
        /// </exception>
        bool TryAcquireReadOnlyLock(IEnumerable<ILockable> resources, int timeout, out IDisposable token);

        /// <summary>
        ///     Acquire exclusive lock on resources
        /// </summary>
        /// <param name="resources">resources to lock</param>
        /// <returns>IDisposable token to release locks on all the resources</returns>
        IDisposable AcquireExclusiveLock(IEnumerable<ILockable> resources);

        /// <summary>
        ///     Try to acquire exclusive lock on all resources and fail if timeout occurs
        /// </summary>
        /// <param name="resources">resources to lock</param>
        /// <param name="timeout">timeout in milliseconds for each resource. -1 is reserved for indefinite wait.</param>
        /// <param name="token">IDisposable token to release locks on all the resources</param>
        /// <returns>If lock is taken</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when timeout is negative, but it is not equal to Infinite (-1),
        ///     which is the only negative value allowed.
        /// </exception>
        bool TryAcquireExclusiveLock(IEnumerable<ILockable> resources, int timeout, out IDisposable token);

        /// <summary>
        ///     Release the lock on all resources
        /// </summary>
        /// <param name="resources">resources to release lock</param>
        void ReleaseLock(IEnumerable<ILockable> resources);
    }
}