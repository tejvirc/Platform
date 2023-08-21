namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     A <see cref="GamePlayCollisionException" /> is thrown when attempting to enable a game or activate a denomination
    ///     that would result in two or more games with the same theme and denomination being accessible to a player
    /// </summary>
    [Serializable]
    public class GamePlayCollisionException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayCollisionException" /> class.
        /// </summary>
        public GamePlayCollisionException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayCollisionException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public GamePlayCollisionException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayCollisionException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">
        ///     The exception that is the cause of the current exception, or a null reference if no inner exception
        ///     is specified.
        /// </param>
        public GamePlayCollisionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayCollisionException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the
        ///     exception being thrown
        /// </param>
        /// <param name="context">
        ///     The System.Runtime.Serialization.StreamingContext that contains contextual information about the
        ///     source or destination
        /// </param>
        protected GamePlayCollisionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}