namespace Aristocrat.Monaco.Application.Contracts.Input
{
    using Kernel;

    /// <summary>
    ///     This event is fired whenever the on screen keyboard is opened
    /// </summary>
    public class OnscreenKeyboardOpenedEvent : BaseEvent
    {
        /// <inheritdoc />
        public OnscreenKeyboardOpenedEvent(object targetControl)
        {
            TargetControl = targetControl;
        }

        /// <summary>
        ///     Gets the target control
        /// </summary>
        public object TargetControl { get; }
    }
}