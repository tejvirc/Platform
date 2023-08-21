namespace Aristocrat.Monaco.Gaming.Contracts.Lobby
{
    /// <summary>
    ///     Definition of the TimeLimitDialogVisibleEvent class
    /// </summary>
    public class TimeLimitDialogVisibleEvent : TimeLimitDialogVisibilityChangedEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeLimitDialogVisibleEvent" /> class.
        /// </summary>
        /// <param name="isFirstPrompt">Is this the first prompt.</param>
        /// <param name="isLastPrompt">Is this the last prompt.</param>
        public TimeLimitDialogVisibleEvent(bool isFirstPrompt, bool isLastPrompt)
            : base(isFirstPrompt, isLastPrompt)
        {
        }
    }
}
