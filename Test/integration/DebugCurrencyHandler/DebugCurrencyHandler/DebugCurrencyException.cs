namespace Vgt.Client12.Testing.Tools
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the DebugCurrencyException class.  This exception should be
    ///     thrown whenever the DebugCurrencyHandler encounters a fatal error.
    /// </summary>
    [Serializable]
    public class DebugCurrencyException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the DebugCurrencyException class.
        /// </summary>
        public DebugCurrencyException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the DebugCurrencyException class and initializes
        ///     the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public DebugCurrencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the DebugCurrencyException class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public DebugCurrencyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the DebugCurrencyException class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a DebugCurrencyException.</param>
        /// <param name="context">Information on the streaming context for a DebugCurrencyException.</param>
        protected DebugCurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}