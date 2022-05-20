namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    /// <summary>
    ///     An interface through which Hard CashOut Lock can be handled
    /// </summary>
    public interface IHardCashOutLock
    {
        /// <summary>
        ///     Gets or sets a value indicating whether a lock up is in progress.
        /// </summary>
        bool Locked { get; set; }

        /// <summary>
        ///     Gets a value indicating whether it's waiting for Key Off
        /// </summary>
        bool WaitingForKeyOff { get; }

        /// <summary>
        ///     Gets whether or not we can currently cashout while locked up
        /// </summary>
        bool CanCashOut { get; }

        /// <summary>
        ///     Recovers the lockup and transaction.
        ///     Handles all internal needs and is done asynchronously.
        /// </summary>
        /// <returns>A value indicating if recovery is needed.</returns>
        bool Recover();

        /// <summary>
        ///     Locks the machine and waits for a key-off. It then transfers off
        ///     all remaining money.
        /// </summary>
        /// <returns>Whether the lock will occur.</returns>
        bool LockupAndCashOut();

        /// <summary>
        ///     Presents the lockup message.
        /// </summary>
        void PresentLockup();

        /// <summary>
        ///     Removes the lockup presentation.
        /// </summary>
        void RemoveLockupPresentation();

        /// <summary>
        ///     Sets the auto reset event and release the transaction.
        ///     Used if an Aft is coming through.
        /// </summary>
        void Set();

        /// <summary>
        ///     Called when the lock is keyed off.
        /// </summary>
        void OnKeyedOff();
    }
}
