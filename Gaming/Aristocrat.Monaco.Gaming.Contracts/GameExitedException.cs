namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the GameExitedException class. This exception indicates that
    ///     the game has exited for unknown reason
    /// </summary>
    [Serializable]
    public class GameExitedException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameExitedException" /> class.
        /// </summary>
        public GameExitedException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameExitedException" /> class.
        ///     Initializes a new instance of the GameExitedException
        ///     class and initializes the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public GameExitedException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameExitedException" /> class.
        ///     Initializes a new instance of the GameExitedException class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public GameExitedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameExitedException" /> class.
        ///     Initializes a new instance of the GameExitedException class and guarantees
        ///     that the object can be serialized correctly. This is mainly to silence FXCop's complaining
        /// </summary>
        /// <param name="serializationInfo">Info required for serialization</param>
        /// <param name="context">
        ///     Describes the source and destination of a given serialized stream,
        ///     and provides an additional caller-defined context.
        /// </param>
        protected GameExitedException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }
    }
}