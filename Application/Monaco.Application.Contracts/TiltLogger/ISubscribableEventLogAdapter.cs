namespace Aristocrat.Monaco.Application.Contracts.TiltLogger
{
    using System;

    /// <summary>
    ///     Interface to extract logs from event information.
    ///     Fires an event when the log is appended.
    /// </summary>
    public interface ISubscribableEventLogAdapter : IEventLogAdapter
    {
        /// <summary>
        ///     The event to notify that a new event is appended into the log.
        /// </summary>
        event EventHandler<TiltLogAppendedEventArgs> Appended;
    }
}
