namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    using Kernel;

    /// <summary>
    ///     This event is fired whenever the onscreen keyboard is opened
    /// </summary>
    public class OnscreenKeyboardOpenedEvent : BaseEvent
    {
        /// <inheritdoc />
        public OnscreenKeyboardOpenedEvent(bool isTextBoxControl = false)
        {
            IsTextBoxControl = isTextBoxControl;
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="OnscreenKeyboardOpenedEvent" /> was posted by a TextBox control.
        /// </summary>
        public bool IsTextBoxControl { get; }
    }
}