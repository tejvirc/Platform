namespace Aristocrat.Monaco.Gaming.UI.Events
{
    using Kernel;
    using Contracts;

    /// <summary>
    ///     Definition of the MessageOverlayDataEvent class.  Test purposes only for now.
    /// </summary>
    public class MessageOverlayDataEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageOverlayDataEvent" /> class.
        /// </summary>
        /// <param name="message">The MessageOverlayData object that was added.</param>
        public MessageOverlayDataEvent(IMessageOverlayData message)
        {
            Message = message;
        }

        /// <summary>
        ///     Gets the MessageOverlayData object
        /// </summary>
        public IMessageOverlayData Message { get; }
    }
}
