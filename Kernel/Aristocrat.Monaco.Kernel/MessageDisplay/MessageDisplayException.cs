namespace Aristocrat.Monaco.Kernel.MessageDisplay
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the MessageDisplayException class.
    /// </summary>
    [Serializable]
    public class MessageDisplayException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageDisplayException" /> class.
        /// </summary>
        public MessageDisplayException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageDisplayException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public MessageDisplayException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageDisplayException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public MessageDisplayException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageDisplayException" /> class.
        /// </summary>
        /// <param name="info">Information on how to serialize a MessageDisplayException.</param>
        /// <param name="context">Information on the streaming context for a MessageDisplayException.</param>
        protected MessageDisplayException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}