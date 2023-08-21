namespace Aristocrat.Monaco.Bingo.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the <see cref="BingoReportGameOutcomeException"/> class.  This exception should be
    ///     thrown whenever an error is encountered related to reporting of the bingo game outcome
    /// </summary>
    [Serializable]
    public class BingoReportGameOutcomeException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BingoReportGameOutcomeException" /> class.
        /// </summary>
        public BingoReportGameOutcomeException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BingoReportGameOutcomeException" /> class and initializes the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public BingoReportGameOutcomeException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BingoReportGameOutcomeException" /> class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public BingoReportGameOutcomeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BingoReportGameOutcomeException" /> class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a BingoSerialNumberException.</param>
        /// <param name="context">Information on the streaming context for a BingoSerialNumberException.</param>
        protected BingoReportGameOutcomeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}