namespace Aristocrat.Monaco.Accounting.Contracts.TransferOut
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    ///     A <see cref="TransferOutException" /> is thrown when attempting to enable a game or activate a denomination
    ///     that would result in two or more games with the same theme and denomination being accessible to a player
    /// </summary>
    [Serializable]
    public class TransferOutException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferOutException" /> class.
        /// </summary>
        public TransferOutException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferOutException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TransferOutException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferOutException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="canRecover">true if this transfer out can be recovered</param>
        public TransferOutException(string message, bool canRecover)
            : base(message)
        {
            CanRecover = canRecover;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferOutException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">
        ///     The exception that is the cause of the current exception, or a null reference if no inner exception
        ///     is specified.
        /// </param>
        public TransferOutException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferOutException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the
        ///     exception being thrown
        /// </param>
        /// <param name="context">
        ///     The System.Runtime.Serialization.StreamingContext that contains contextual information about the
        ///     source or destination
        /// </param>
        protected TransferOutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        ///     Gets a value indicating whether or not this transaction can be recovered
        /// </summary>
        public bool CanRecover { get; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}