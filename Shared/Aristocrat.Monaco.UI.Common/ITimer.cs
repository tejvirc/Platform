namespace Aristocrat.Monaco.UI.Common
{
    using System;

    /// <summary>
    ///     Interface for a dispatcher timer to facilitate unit tests.
    /// </summary>
    public interface ITimer
    {
        /// <summary>Gets or sets the timer interval.</summary>
        TimeSpan Interval { get; set; }

        /// <summary>Gets or sets a value indicating whether the timer is enabled.</summary>
        bool IsEnabled { get; set; }

        /// <summary>The timer Tick event.</summary>
        event EventHandler Tick;

        /// <summary>Starts the timer.</summary>
        void Start();

        /// <summary>Stops the timer.</summary>
        void Stop();
    }
}