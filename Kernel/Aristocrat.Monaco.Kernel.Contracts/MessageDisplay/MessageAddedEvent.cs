namespace Aristocrat.Monaco.Kernel
{
    using System;

    /// <summary>
    ///     Definition of the MessageAddedEvent class.  Test purposes only for now.
    /// </summary>
    [Serializable]
    public class MessageAddedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageAddedEvent" /> class.
        /// </summary>
        /// <param name="message">The DisplayableMessage object that was added.</param>
        public MessageAddedEvent(DisplayableMessage message)
        {
            Message = message;
        }

        /// <summary>
        ///     Gets the DisplayableMessage object
        /// </summary>
        public DisplayableMessage Message { get; }
    }
}