namespace Aristocrat.Monaco.Kernel.Contracts.LockManagement
{
    using System;

    /// <summary>
    ///     Read write locking mechanism to provide syncronised access to the resource
    /// </summary>
    public interface IReadWriteLockable : ILockable
    {
        /// <summary>
        ///     Take read only lock on the resource
        /// </summary>
        /// <returns>an IDisposable token which releases lock on dispose</returns>
        IDisposable AcquireReadOnlyLock();

        /// <summary>
        ///     Try to take read only lock on the resource and fail if timeout occurs
        /// </summary>
        /// <param name="timeout">timeout in milliseconds, -1(indefinite timeout) is the only allowed negative number.</param>
        /// <param name="disposableToken">an IDisposable token which releases lock on dispose</param>
        /// <returns>If the lock is taken</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when timeout is negative, but it is not equal to Infinite (-1),
        ///     which is the only negative value allowed.
        /// </exception>
        bool TryAcquireReadOnlyLock(int timeout, out IDisposable disposableToken);
    }
}