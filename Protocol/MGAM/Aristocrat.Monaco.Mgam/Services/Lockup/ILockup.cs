namespace Aristocrat.Monaco.Mgam.Services.Lockup
{
    using Kernel;

    /// <summary>
    ///     Define the <see cref="ILockup" /> interface.
    /// </summary>
    public interface ILockup
    {
        /// <summary>
        ///     Get whether a lockup by host is active.
        /// </summary>
        bool IsLockedByHost { get; }

        /// <summary>
        ///     Get whether a lockup is active which required employee card to clear.
        /// </summary>
        bool IsLockedForEmployeeCard { get; }

        /// <summary>
        ///     Gets a value that indicates whether an employee is logged in.
        /// </summary>
        bool IsEmployeeLoggedIn { get; }

        /// <summary>
        ///     Add a lock condition from protocol command.
        /// </summary>
        /// <param name="message">Displayable message.</param>
        void AddHostLock(string message);

        /// <summary>
        ///     Clear a lock condition from protocol command.
        /// </summary>
        void ClearHostLock();

        /// <summary>
        ///     Force condition where employee card is required to clear lockup.
        /// </summary>
        void LockupForEmployeeCard(string message = null, SystemDisablePriority priority = SystemDisablePriority.Immediate);
    }
}