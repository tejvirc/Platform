namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     The TimeLimitDialogVisibilityChangedEvent is published when the time limit dialog visibility changes.
    /// </summary>
    [Serializable]
    public class DisableCountdownTimerEvent : BaseEvent
    {
        private const int DefaultCountdownTimeInMinutes = 5;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisableCountdownTimerEvent" /> class.
        /// </summary>
        /// <param name="start">Are we starting or stopping the countdown?  True == starting.</param>
        public DisableCountdownTimerEvent(bool start)
            : this(start, TimeSpan.FromMinutes(DefaultCountdownTimeInMinutes))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisableCountdownTimerEvent" /> class.
        /// </summary>
        /// <param name="start">Are we starting or stopping the countdown?  True == starting.</param>
        /// <param name="countdownTime">Amount of time for the countdown.</param>
        public DisableCountdownTimerEvent(bool start, TimeSpan countdownTime)
        {
            Start = start;
            CountdownTime = start ? countdownTime : TimeSpan.Zero;
        }

        /// <summary>
        ///     Gets a value indicating whether we are starting or stopping the countdown timer
        ///     True == starting.
        /// </summary>
        public bool Start { get; }

        /// <summary>
        ///     Gets a value indicating the amount of time for the countdown timer.
        ///     This is only applicable if Start is true.
        /// </summary>
        public TimeSpan CountdownTime { get; }
    }
}
