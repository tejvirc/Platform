namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the CurrencyInException class.  This exception should be
    ///     thrown whenever the CurrencyInHandler encounters a fatal error.
    /// </summary>
    [Serializable]
    public class CurrencyInException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyInException" /> class.
        /// </summary>
        public CurrencyInException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyInException" /> class.
        ///     Initializes a new instance of the CurrencyInException class and initializes the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public CurrencyInException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyInException" /> class.
        ///     Initializes a new instance of the CurrencyInException class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exeception to set as InnerException.</param>
        public CurrencyInException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyInException" /> class.
        ///     Initializes a new instance of the CurrencyInException class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a CurrencyInException.</param>
        /// <param name="context">Information on the streaming context for a CurrencyInException.</param>
        protected CurrencyInException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}