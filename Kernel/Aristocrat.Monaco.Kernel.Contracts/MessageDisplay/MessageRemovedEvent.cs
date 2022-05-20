namespace Aristocrat.Monaco.Kernel
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
        public MessageRemovedEvent(DisplayableMessage message)
        {
            Message = message;
        }

        /// <summary>
        ///     Gets the DisplayableMessage object
        /// </summary>
        public DisplayableMessage Message { get; }
    }
}