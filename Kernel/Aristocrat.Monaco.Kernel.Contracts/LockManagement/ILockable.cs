namespace Aristocrat.Monaco.Kernel.Contracts.LockManagement
{
    using System;

    /// <summary>
    ///     Provides lock management for the resource
    /// </summary>
    public interface ILockable
    {
        /// <summary>
        ///     Unique name given to resource to sort on while deciding on locking order
        /// </summary>
        string UniqueLockableName { get; }

        /// <summary>
        ///     Take exclusive lock on the resource
        /// </summary>
        /// <returns>an IDisposable token which releases lock on dispose</returns>
        IDisposable AcquireExclusiveLock();

        /// <summary>
        ///     Exits from last lock taken
        /// </summary>
        void ReleaseLock();

        /// <summary>
        ///     Try to take exclusive lock on the resource and fail if timeout occurs
        /// </summary>
        /// <param name="timeout">timeout in milliseconds, -1(indefinite timeout) is the only allowed negative number.</param>
        /// <param name="disposableToken">an IDisposable token which releases lock on dispose</param>
        /// <returns>If the lock is taken</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when timeout is negative, but it is not equal to Infinite (-1),
        ///     which is the only negative value allowed.
        /// </exception> 
        bool TryAcquireExclusiveLock(int timeout, out IDisposable disposableToken);
    }
}