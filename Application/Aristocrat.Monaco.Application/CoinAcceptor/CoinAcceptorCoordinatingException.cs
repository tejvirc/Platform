namespace Aristocrat.Monaco.Application.CoinAcceptor
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the CoinAcceptorCoordinatingException class.
    /// </summary>
    [Serializable]
    public class CoinAcceptorCoordinatingException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinAcceptorCoordinatingException" /> class.
        /// </summary>
        public CoinAcceptorCoordinatingException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinAcceptorCoordinatingException" /> class and initializes
        ///     the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public CoinAcceptorCoordinatingException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinAcceptorCoordinatingException" /> class.
        ///     Initializes a new instance of the CoinAcceptorCoordinatingException class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public CoinAcceptorCoordinatingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinAcceptorCoordinatingException" /> class.
        ///     Initializes a new instance of the CoinAcceptorCoordinatingException class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a VgtTransactionDispatchingException.</param>
        /// <param name="context">Information on the streaming context for a VgtTransactionDispatchingException.</param>
        protected CoinAcceptorCoordinatingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
