namespace Aristocrat.Monaco.Gaming.Contracts.Lobby
{
    using Kernel;

    /// <summary>
    ///     The TimeLimitDialogVisibilityChangedEvent is published when the time limit dialog
    ///     visibility changes.
    /// </summary>
    public class TimeLimitDialogVisibilityChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeLimitDialogVisibilityChangedEvent" /> class.
        /// </summary>
        /// <param name="isFirstPrompt">Is this the first prompt.</param>
        /// <param name="isLastPrompt">Is this the last prompt.</param>
        public TimeLimitDialogVisibilityChangedEvent(bool isFirstPrompt, bool isLastPrompt)
        {
            IsFirstPrompt = isFirstPrompt;
            IsLastPrompt = isLastPrompt;
        }

        /// <summary>
        ///     Gets a value indicating whether this is the first time limit prompt in the session. This is
        ///     only applicable if IsVisible is true.
        /// </summary>
        public bool IsFirstPrompt { get; }

        /// <summary>
        ///     Gets a value indicating whether this is the last time limit prompt in the session.
        ///     Some Jurisdictions force a cash out after a certain time period.  This is
        ///     only applicable if IsVisible is true.
        /// </summary>
        public bool IsLastPrompt { get; }
    }
}
