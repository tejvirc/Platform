namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    using Kernel;

    /// <summary>
    ///     This event is fired whenever the on screen keyboard is closed
    /// </summary>
    public class OnscreenKeyboardClosedEvent : BaseEvent
    {
        /// <inheritdoc />
        public OnscreenKeyboardClosedEvent(bool isTextBoxControl = false)
        {
            IsTextBoxControl = isTextBoxControl;
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="OnscreenKeyboardClosedEvent" /> was posted by a TextBox control.
        /// </summary>
        public bool IsTextBoxControl { get; }
    }
}