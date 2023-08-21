namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    /// Definition of the IAftLockHandler interface.
    /// </summary>
    public interface IAftLockHandler
    {
        /// <summary>
        ///     The event to notify a lock has been acquired.
        /// </summary>
        /// <remarks>
        ///   Hook up to this event if a component is sensitive to the lock handling.
        /// </remarks>
        event EventHandler<EventArgs> OnLocked;

        /// <summary>Gets a value indicating the current status of the lock.</summary>
        AftGameLockStatus LockStatus { get; }

        /// <summary>
        ///     Requests an Aft lock or unlock to the PT.
        /// </summary>
        /// <param name="requestLock">True to lock the PT, false to unlock.</param>
        /// <param name="timeout">The lock timeout in milliseconds.</param>
        void AftLock(bool requestLock, uint timeout);

        /// <summary>
        ///     Gets the transaction ID associated with the locked, and transfers ownership to the
        ///     caller. The caller then is responsible for releasing the Guid.
        ///     If the lock handler is unlocked, returns Guid.Empty.
        /// </summary>
        /// <returns>The transaction ID.</returns>
        Guid RetrieveTransactionId();
        
        /// <summary>
        ///     AFT lock transfer conditions
        /// </summary>
        AftTransferConditions AftLockTransferConditions { get; set; }
    }
}
