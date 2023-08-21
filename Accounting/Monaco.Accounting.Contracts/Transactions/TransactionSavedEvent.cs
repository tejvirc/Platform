namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Globalization;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Definition of the TransactionSavedEvent class.
    /// </summary>
    /// <remarks>
    ///     An event of this type is posted when a transaction (i.e. implementation of ITransaction) is saved by the
    ///     TransactionHistory component.
    /// </remarks>
    [ProtoContract]
    public class TransactionSavedEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public TransactionSavedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionSavedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The transaction that was saved.</param>
        public TransactionSavedEvent(ITransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets a reference to the transaction that was saved.
        /// </summary>
        [ProtoMember(1)]
        public ITransaction Transaction { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Transaction={1}]",
                GetType().Name,
                Transaction);
        }
    }
}