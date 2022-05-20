namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of an InvalidGameException.  The InvalidGameException is thrown when a game fails to launch for any
    ///     reason.
    /// </summary>
    [Serializable]
    public class InvalidGameException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidGameException" /> class.
        /// </summary>
        public InvalidGameException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidGameException" /> class.
        ///     Initializes a new instance of the InvalidGameException
        ///     class and initializes the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public InvalidGameException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidGameException" /> class.
        ///     Initializes a new instance of the InvalidGameException class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public InvalidGameException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidGameException" /> class.
        ///     Initializes a new instance of the InvalidGameException class and guarantees
        ///     that the object can be serialized correctly. This is mainly to silence FXCop's complaining
        /// </summary>
        /// <param name="serializationInfo">Info required for serialization</param>
        /// <param name="context">
        ///     Describes the source and destination of a given serialized stream,
        ///     and provides an additional caller-defined context.
        /// </param>
        protected InvalidGameException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }
    }
}