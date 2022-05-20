namespace Aristocrat.Monaco.Gaming.Contracts.Meters
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the GameMeterException class.
    /// </summary>
    [Serializable]
    public class GameMeterException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameMeterException" /> class.
        /// </summary>
        public GameMeterException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameMeterException" /> class.
        ///     Initializes a new instance of the GameMeterException class and initializes
        ///     the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public GameMeterException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameMeterException" /> class.
        ///     Initializes a new instance of the GameMeterException class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public GameMeterException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameMeterException" /> class.
        ///     Initializes a new instance of the GameMeterException class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a VgtTransactionDispatchingException.</param>
        /// <param name="context">Information on the streaming context for a VgtTransactionDispatchingException.</param>
        protected GameMeterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}