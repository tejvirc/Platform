namespace Aristocrat.Monaco.Kernel
{
    using ProtoBuf;
    using System;

    /// <summary>
    ///     Definition of the MessageRemovedEvent class.
    /// </summary>
    [ProtoContract]
    public class MessageRemovedEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserializations
        /// </summary>
        public MessageRemovedEvent()
        {
        }

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
        [ProtoMember(1)]
        public DisplayableMessage Message { get; }
    }
}