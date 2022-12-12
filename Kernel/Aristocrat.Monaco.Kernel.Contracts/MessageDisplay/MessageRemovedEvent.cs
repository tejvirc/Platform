namespace Aristocrat.Monaco.Kernel.Contracts.MessageDisplay
{
    using System;

    /// <summary>
    ///     Definition of the MessageRemovedEvent class.
    /// </summary>
    [Serializable]
    public class MessageRemovedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageRemovedEvent" /> class.
        /// </summary>
        /// <param name="message">The DisplayableMessage object that was removed.</param>
        public MessageRemovedEvent(IDisplayableMessage message)
        {
            Message = message;
        }

        /// <summary>
        ///     Gets the DisplayableMessage object
        /// </summary>
        public IDisplayableMessage Message { get; }
    }
}