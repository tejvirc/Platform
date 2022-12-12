namespace Aristocrat.Monaco.Kernel.MessageDisplay
{
    
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using Contracts.MessageDisplay;

    /// <summary>
    ///     Encapsulates the data associated with a message to display to a user.
    /// </summary>
    [Serializable]
    public class DisplayableMessage : ISerializable, IDisplayableMessage
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
        /// <param name="message">The message text to display, the result message depends on the specified name of the culture provider(PlayerCultureProvider|OperatorCultureProvider).</param>
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
            if (message != null)
            {
                MessageCallback = message;
                message();
            }

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

        /// <inheritdoc />
        public string Message => MessageCallback != null ? MessageCallback.Invoke() : throw new Exception("Message callbacks are not initialized.");

        /// <inheritdoc />
        public Func<string> MessageCallback { get; set; }

        /// <inheritdoc />
        public string HelpText => HelpTextCallback?.Invoke();

        /// <summary>
        ///     Gets or sets the help text callback
        /// </summary>
        public Func<string> HelpTextCallback { get; set; }

        /// <inheritdoc />
        public DisplayableMessageClassification Classification { get; }

        /// <inheritdoc />
        public DisplayableMessagePriority Priority { get; }

        /// <inheritdoc />
        public Type ReasonEvent { get; }

        /// <inheritdoc />
        public Guid Id { get; }

        /// <inheritdoc />
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
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("Message", Message);
            info.AddValue("HelpText", HelpText);
            info.AddValue("Classification", Classification);
            info.AddValue("Priority", Priority);
            info.AddValue("ReasonEvent", ReasonEvent);
            info.AddValue("Id", Id);
            info.AddValue("MessageHasDynamicGuid", MessageHasDynamicGuid);
        }


        /// <inheritdoc />
        public bool IsMessageEquivalent(IDisplayableMessage message)
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