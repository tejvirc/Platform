namespace Aristocrat.Monaco.Bingo.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the <see cref="BingoGamePlayException"/> class.  This exception should be
    ///     thrown whenever an error is encountered related to the bingo game play
    /// </summary>
    [Serializable]
    public class BingoGamePlayException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BingoGamePlayException" /> class.
        /// </summary>
        public BingoGamePlayException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BingoGamePlayException" /> class and initializes the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public BingoGamePlayException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BingoGamePlayException" /> class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public BingoGamePlayException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BingoGamePlayException" /> class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a BingoSerialNumberException.</param>
        /// <param name="context">Information on the streaming context for a BingoSerialNumberException.</param>
        protected BingoGamePlayException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}