namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Timers;

    /// <summary>
    ///     Interface for the SystemTimerWrapper to facilitate unit tests by enabling mocking through an interface.
    ///     It is a wrapper over <see cref="System.Timers"/>.
    /// </summary>
    public interface ISystemTimerWrapper : IDisposable
    {
        /// <summary>
        ///     Gets or sets a Boolean indicating whether the Timer should raise the Elapsed event only once (false) or repeatedly
        ///     (true).
        /// </summary>
        bool AutoReset { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the Timer should raise the Elapsed event.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the interval, expressed in milliseconds, at which to raise the Elapsed event.
        /// </summary>
        double Interval { get; set; }

        /// <summary>
        ///     Starts raising the Elapsed event by setting Enabled to true.
        /// </summary>
        void Start();

        /// <summary>
        ///     Stops raising the Elapsed event by setting Enabled to false.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Occurs when the interval elapses.
        /// </summary>
        event ElapsedEventHandler Elapsed;
    }
}