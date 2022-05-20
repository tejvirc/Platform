namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the TransactionException class.
    /// </summary>
    [Serializable]
    public class TransactionException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionException" /> class.
        /// </summary>
        public TransactionException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionException" /> class.
        ///     Initializes a new instance of the TransactionException class and initializes
        ///     the contained message.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public TransactionException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionException" /> class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exeception to set as InnerException.</param>
        public TransactionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionException" /> class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a TransactionException.</param>
        /// <param name="context">Information on the streaming context for a TransactionException.</param>
        protected TransactionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}