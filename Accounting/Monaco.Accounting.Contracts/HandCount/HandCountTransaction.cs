namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;
    using Transactions;
    using TransferOut;

    /// <summary>
    ///     Definition of the WatTransaction class
    /// </summary>
    public class HandCountTransaction : BaseTransaction, ITransactionContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HandCountTransaction" /> class.  This constructor is only used.
        ///     by the transaction framework.
        /// </summary>
        public HandCountTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandCountTransaction" /> class.
        /// </summary>
        /// <param name="transactionId">bank transaction id</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="amount">The amount requiring</param>
        /// <param name="reason">The transfer out reason</param>
        public HandCountTransaction(
            Guid transactionId,
            DateTime transactionDateTime,
            long amount,
            TransferOutReason reason = TransferOutReason.CashOut)
            : base(0, transactionDateTime)
        {
            BankTransactionId=transactionId;
            Amount = amount;
            Reason = reason;
        }

        /// <summary>
        ///     Gets the amount of money involved in the transaction.
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Gets or sets the associated bank transaction Id
        /// </summary>
        public Guid BankTransactionId { get; set; }

        /// <summary>
        ///     Gets the reason for the transfer out
        /// </summary>
        public TransferOutReason Reason { get; private set; }

        /// <inheritdoc />
        public override string Name => Localizer.For(CultureFor.Player).GetString(ResourceKeys.HandCountTransactionName);

        /// <inheritdoc />
        public long TransactionAmount => Amount;

        /// <inheritdoc />
        public Guid TraceId { get; set; }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="handCountTransaction1">The first transaction</param>
        /// <param name="handCountTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(HandCountTransaction handCountTransaction1, HandCountTransaction handCountTransaction2)
        {
            if (ReferenceEquals(handCountTransaction1, handCountTransaction2))
            {
                return true;
            }

            if (handCountTransaction1 is null || handCountTransaction2 is null)
            {
                return false;
            }

            return handCountTransaction1.Equals(handCountTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="handCountTransaction1">The first transaction</param>
        /// <param name="handCountTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(HandCountTransaction handCountTransaction1, HandCountTransaction handCountTransaction2)
        {
            return !(handCountTransaction1 == handCountTransaction2);
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            Amount = (long)values["Amount"];
            Reason = (TransferOutReason)values["Reason"];

            var bankTransId = values["BankTransactionId"];
            if (bankTransId != null)
            {
                BankTransactionId = (Guid)bankTransId;
            }

            var traceId = values["TraceId"];
            if (traceId != null)
            {
                TraceId = (Guid)traceId;
            }

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);
            using (var transaction = block.StartTransaction())
            {
                transaction[element, "Amount"] = Amount;
                transaction[element, "TraceId"] = TraceId;
                transaction[element, "Reason"] = Reason;
                transaction[element, "BankTransactionId"] = BankTransactionId;

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} LogSequence={1}, DateTime={2}, TransactionId={3}, Amount={4}, Reason={5}]",
                GetType(),
                LogSequence,
                TransactionDateTime.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                Amount,
                Reason);

            return builder.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var handCountTransaction = obj as HandCountTransaction;
            return handCountTransaction != null && Equals(handCountTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        public override object Clone()
        {
            var copy = new HandCountTransaction(
                BankTransactionId,
                TransactionDateTime,
                Amount,
                Reason)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                TraceId = TraceId
            };

            return copy;
        }

        /// <summary>
        ///     Checks that two HandCountTransaction are the same by value.
        /// </summary>
        /// <param name="handCountTransaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(HandCountTransaction handCountTransaction)
        {
            return handCountTransaction != null &&
                   base.Equals(handCountTransaction);
        }
    }
}