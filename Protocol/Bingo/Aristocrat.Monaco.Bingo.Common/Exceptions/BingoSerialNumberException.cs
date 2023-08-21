namespace Aristocrat.Monaco.Bingo.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the BingoSerialNumberException class.  This exception should be
    ///     thrown whenever a fatal error is encountered related to the bingo card serial
    ///     numbers.
    /// </summary>
    [Serializable]
    public class BingoSerialNumberException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BingoSerialNumberException" /> class.
        /// </summary>
        public BingoSerialNumberException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the BingoSerialNumberException class and initializes the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public BingoSerialNumberException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the BingoSerialNumberException class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public BingoSerialNumberException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the BingoSerialNumberException class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a BingoSerialNumberException.</param>
        /// <param name="context">Information on the streaming context for a BingoSerialNumberException.</param>
        protected BingoSerialNumberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}