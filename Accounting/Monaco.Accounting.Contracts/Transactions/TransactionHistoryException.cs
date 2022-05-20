namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    ///     Throw when there is a problem with the TransactionHistory.
    /// </summary>
    /// <remarks>
    ///     When thrown, this exception should contain a reference to the transaction for which there was a problem.
    /// </remarks>
    [Serializable]
    public class TransactionHistoryException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionHistoryException" /> class.
        /// </summary>
        public TransactionHistoryException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionHistoryException" /> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        public TransactionHistoryException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionHistoryException" /> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TransactionHistoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionHistoryException" /> class.
        /// </summary>
        /// <param name="serializationInfo">The SerializationInfo.</param>
        /// <param name="streamingContext">The StreamingContext.</param>
        protected TransactionHistoryException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        /// <summary>
        ///     Gets the transaction that has been attached to the TransactionHistoryException.
        /// </summary>
        public ITransaction Transaction { get; private set; }

        /// <summary>
        ///     Attaches a transaction to the exception.
        /// </summary>
        /// <param name="transaction">The transaction to attach.</param>
        public void AttachTransaction(ITransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Method to serialize the stored data for a TransactionHistoryException.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}