namespace Aristocrat.Monaco.Application.Contracts
{
    using Aristocrat.Monaco.Kernel.Contracts.LockManagement;
    using System;

    /// <summary>
    ///     This <c>enum</c> type defines the time frame in recording a meter.
    /// </summary>
    public enum MeterTimeframe
    {
        /// <summary>
        ///     A meter in this time frame should have been recorded since the very
        ///     beginning, e.g, the game terminal's first time to run.
        /// </summary>
        Lifetime,

        /// <summary>
        ///     A meter in this time frame can be reset programatically - e.g, whenver the
        ///     note acceptor stacker box is removed.
        /// </summary>
        Period,

        /// <summary>
        ///     A meter in this time frame is reset on a session basis - e.g, whenever the
        ///     terminal is reboot.
        /// </summary>
        Session
    }

    /// <summary>
    ///     An interface by which a meter can be incremented and its values for each time frame can be retrieved.
    /// </summary>
    public interface IMeter : IReadWriteLockable
    {
        /// <summary>
        ///     Gets the name of the meter
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the classification of the meter
        /// </summary>
        MeterClassification Classification { get; }

        /// <summary>
        ///     Gets the Lifetime meter value
        /// </summary>
        long Lifetime { get; }

        /// <summary>
        ///     Gets the Period meter value
        /// </summary>
        long Period { get; }

        /// <summary>
        ///     Gets the Session meter value
        /// </summary>
        long Session { get; }

        /// <summary>
        ///     The event to notify a meter value has been changed.
        /// </summary>
        /// <remarks>
        ///     Hook up to this event if a component is sensitive to the meter value change.
        /// </remarks>
        event EventHandler<MeterChangedEventArgs> MeterChangedEvent;

        /// <summary>
        ///     Increments the Lifetime, Period and Session values for this meter
        /// </summary>
        /// <param name="amount">The amount to increment the meter by</param>
        void Increment(long amount);
    }
}