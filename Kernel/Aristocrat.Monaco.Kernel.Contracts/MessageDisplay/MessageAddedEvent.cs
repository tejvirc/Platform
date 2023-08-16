namespace Aristocrat.Monaco.Kernel
{
    using ProtoBuf;
    using System;

    /// <summary>
    ///     Definition of the MessageAddedEvent class.  Test purposes only for now.
    /// </summary>
    [ProtoContract]
    public class MessageAddedEvent : BaseEvent
    {

        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public MessageAddedEvent()
        {
        }

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
        [ProtoMember(1)]
        public DisplayableMessage Message { get; }
    }
}