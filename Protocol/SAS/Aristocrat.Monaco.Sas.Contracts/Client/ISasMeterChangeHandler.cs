namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///    Definition of the ISasMeterChangeHandler interface.
    /// </summary>
    public interface ISasMeterChangeHandler
    {
        /// <summary>
        ///     The event to notify a meter change is ready to occur
        /// </summary>
        /// <remarks>
        ///     Hook up to this event if a component is sensitive to the meter change handling.
        /// </remarks>
        event EventHandler<EventArgs> OnChangeCommit;

        /// <summary>
        ///     The event to notify pending meter changes are to be cancelled
        /// </summary>
        /// <remarks>
        ///     Hook up to this event if a component is sensitive to the meter change handling.
        /// </remarks>
        event EventHandler<EventArgs> OnChangeCancel;

        /// <summary>Gets a value indicating the current status of the lock.</summary>
        MeterCollectStatus MeterChangeStatus { get; }

        /// <summary>
        ///     Requests an meter lifetime change
        /// </summary>
        /// <param name="status">Meter collect status for the change</param>
        /// <param name="timeoutValue">The time where the pending change will be cancelled</param>
        void StartPendingChange(MeterCollectStatus status, double timeoutValue);

        /// <summary>
        ///     Acknowledgement of pending change by host
        /// </summary>
        void AcknowledgePendingChange();

        /// <summary>
        ///     Host is ready for pending change to occur
        /// </summary>
        void ReadyForPendingChange();

        /// <summary>
        ///     Requests a cancellation of pending change.  Does nothing if there is no pending change
        /// </summary>
        void CancelPendingChange();
    }
}
