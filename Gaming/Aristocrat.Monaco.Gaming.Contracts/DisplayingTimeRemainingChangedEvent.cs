namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     The DisplayingTimeRemainingChangedEvent is published when the time limit dialog visibility changes.
    /// </summary>
    public class DisplayingTimeRemainingChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayingTimeRemainingChangedEvent" /> class.
        /// </summary>
        /// <param name="isDisplayingTimeRemaining">Are we displaying Time Remaining?</param>
        public DisplayingTimeRemainingChangedEvent(bool isDisplayingTimeRemaining)
        {
            IsDisplayingTimeRemaining = isDisplayingTimeRemaining;
        }

        /// <summary>
        ///     Gets a value indicating whether we are currently displaying the Time Remaining
        ///     in the current Responsible Gaming Session
        /// </summary>
        public bool IsDisplayingTimeRemaining { get; }
    }
}
