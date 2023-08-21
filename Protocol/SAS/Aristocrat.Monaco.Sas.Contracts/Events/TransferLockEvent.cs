namespace Aristocrat.Monaco.Sas.Contracts.Events
{
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Kernel;

    /// <summary>
    ///     Definition of the TransferLockEvent class
    /// </summary>
    public class TransferLockEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferLockEvent" /> class.
        /// </summary>
        /// <param name="locked">If a lock is active</param>
        /// <param name="transferConditions">the allowed types of locks</param>
        public TransferLockEvent(bool locked, AftTransferConditions transferConditions)
        {
            Locked = locked;
            TransferConditions = transferConditions;
        }

        /// <summary>
        ///     Gets a value indicating if a transfer lock is active
        /// </summary>
        public bool Locked { get; }

        /// <summary>
        ///     Gets a value indicating what kind of transfers are
        ///     available to lock
        /// </summary>
        public AftTransferConditions TransferConditions { get; }
    }
}