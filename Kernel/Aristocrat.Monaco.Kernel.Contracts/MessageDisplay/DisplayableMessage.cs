namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    ///     Encapsulates the data associated with a message to display to a user.
    /// </summary>
    [Serializable]
    public class DisplayableMessage : ISerializable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayableMessage" /> class.
        /// </summary>
        /// <param name="message">The message text to display</param>
        /// <param name="classification">The classification of the message</param>
        /// <param name="priority">The priority of the message</param>
        /// <param name="id">The id for this message</param>
        /// <param name="helpText">The information on how to clear a specific lockup</param>
        public DisplayableMessage(
            Func<string> message,
            DisplayableMessageClassification classification,
            DisplayableMessagePriority priority,
            Guid? id = null,
            Func<string> helpText = null)
            : this(message, classification, priority, null, id, helpText)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayableMessage" /> class.
        /// </summary>
        /// <param name="message">The message text to display</param>
        /// <param name="classification">The classification of the message</param>
        /// <param name="priority">The priority of the message</param>
        /// <param name="reason">The event type that was the reason for this message</param>
        /// <param name="id">The id for this message</param>
        /// <param name="helpText">The information on how to clear a specific lockup</param>
        public DisplayableMessage(
            Func<string> message,
            DisplayableMessageClassification classification,
            DisplayableMessagePriority priority,
            Type reason,
            Guid? id = null,
            Func<string> helpText = null)
        {
            MessageCallback = message;
            HelpTextCallback = helpText;
            Classification = classification;
            Priority = priority;
            ReasonEvent = reason;
            Id = id ?? Guid.NewGuid();
            MessageHasDynamicGuid = !id.HasValue;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayableMessage" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization. </param>
        protected DisplayableMessage(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var message = (string)info.GetValue("Message", typeof(string));
            MessageCallback = () => message;
            var helpText = (string)info.GetValue("HelpText", typeof(string));
            HelpTextCallback = () => helpText;
            Classification = (DisplayableMessageClassification)info.GetValue("Classification", typeof(DisplayableMessageClassification));
            Priority = (DisplayableMessagePriority)info.GetValue("Priority", typeof(DisplayableMessagePriority));
            ReasonEvent = (Type)info.GetValue("ReasonEvent", typeof(Type));
            Id = (Guid)info.GetValue("Id", typeof(Guid));
            MessageHasDynamicGuid = (bool)info.GetValue("MessageHasDynamicGuid", typeof(bool));
        }

        /// <summary>
        ///     Gets or sets the message text to display
        /// </summary>
        public string Message => MessageCallback.Invoke();

        /// <summary>
        ///     Gets or sets the message callback
        /// </summary>
        public Func<string> MessageCallback { get; set; }

        /// <summary>
        ///     Gets or sets the help text to display
        /// </summary>
        public string HelpText => HelpTextCallback?.Invoke();

        /// <summary>
        ///     Gets or sets the help text callback
        /// </summary>
        public Func<string> HelpTextCallback { get; set; }

        /// <summary>
        ///     Gets the message classification
        /// </summary>
        public DisplayableMessageClassification Classification { get; }

        /// <summary>
        ///     Gets the message priority
        /// </summary>
        public DisplayableMessagePriority Priority { get; }

        /// <summary>
        ///     Gets the event that was the reason for this message
        /// </summary>
        public Type ReasonEvent { get; }

        /// <summary>
        ///     Gets the error or warning ID for the message
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Boolean indicating if the message has a dynamic GUID generated at the time the message was created.
        /// This is used when determining if messages are equivalent or not.  If there is
        /// a dynamic GUID, we have to use other methods to equate like string matching
        /// </summary>
        public bool MessageHasDynamicGuid { get; }

        /// <summary>
        ///     Builds and returns a string representation of the object instance.
        /// </summary>
        /// <returns>A string representation of the object instance</returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Message=\"{0}\", Classification={1}, Priority={2}",
                Message,
                Classification,
                Priority);
        }

        /// <inheritdoc />
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("Message", MessageCallback?.Invoke());
            info.AddValue("HelpText", HelpText);
            info.AddValue("Classification", Classification);
            info.AddValue("Priority", Priority);
            info.AddValue("ReasonEvent", ReasonEvent);
            info.AddValue("Id", Id);
            info.AddValue("MessageHasDynamicGuid", MessageHasDynamicGuid);
        }

        /// <summary>
        /// IsMessageEquivalent checks to see if the DisplayableMessage if the fields of the passed in DisplayableMessage are
        /// equivalent to the existing message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsMessageEquivalent(DisplayableMessage message)
        {
            // if both of the messages have dynamic Guids, don't try to equate the Guids.
            return (message.Id == Id || message.MessageHasDynamicGuid && MessageHasDynamicGuid) &&
                   message.HelpText == HelpText &&
                   message.Message == Message &&
                   message.Classification == Classification &&
                   message.Priority == Priority &&
                   message.ReasonEvent == ReasonEvent;
        }
    }
}