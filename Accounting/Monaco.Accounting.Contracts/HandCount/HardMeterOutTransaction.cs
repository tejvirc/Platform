namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;
    using Transactions;
    using TransferOut;

    /// <summary>
    ///     Definition of the HardMeterOutTransaction class
    /// </summary>
    public class HardMeterOutTransaction : BaseTransaction, ITransactionContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardMeterOutTransaction" /> class.  This constructor is only used.
        ///     by the transaction framework.
        /// </summary>
        public HardMeterOutTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardMeterOutTransaction" /> class.
        /// </summary>
        /// <param name="transactionId">bank transaction id</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="amount">The amount requiring</param>
        /// <param name="reason">The transfer out reason</param>
        public HardMeterOutTransaction(
            Guid transactionId,
            DateTime transactionDateTime,
            long amount,
            TransferOutReason reason = TransferOutReason.CashOut)
            : base(0, transactionDateTime)
        {
            BankTransactionId = transactionId;
            Amount = amount;
            Reason = reason;
        }

        /// <summary>
        ///     Gets the amount of money involved in the transaction.
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Gets the date/time when hard meter completed
        /// </summary>
        public DateTime HardMeterCompletedDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the state
        /// </summary>
        public HardMeterOutState State { get; set; }

        /// <summary>
        ///     Gets or sets the associated bank transaction Id
        /// </summary>
        public Guid BankTransactionId { get; set; }

        /// <summary>
        ///     Gets the reason for the transfer out
        /// </summary>
        public TransferOutReason Reason { get; private set; }

        /// <inheritdoc />
        public override string Name => Localizer.For(CultureFor.Player).GetString(ResourceKeys.HardMeterOutTransactionName);

        /// <inheritdoc />
        public long TransactionAmount => Amount;

        /// <inheritdoc />
        public Guid TraceId { get; set; }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="hardMeterOutTransaction1">The first transaction</param>
        /// <param name="hardMeterOutTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(HardMeterOutTransaction hardMeterOutTransaction1, HardMeterOutTransaction hardMeterOutTransaction2)
        {
            if (ReferenceEquals(hardMeterOutTransaction1, hardMeterOutTransaction2))
            {
                return true;
            }

            if (hardMeterOutTransaction1 is null || hardMeterOutTransaction2 is null)
            {
                return false;
            }

            return hardMeterOutTransaction1.Equals(hardMeterOutTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="hardMeterOutTransaction1">The first transaction</param>
        /// <param name="hardMeterOutTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(HardMeterOutTransaction hardMeterOutTransaction1, HardMeterOutTransaction hardMeterOutTransaction2)
        {
            return !(hardMeterOutTransaction1 == hardMeterOutTransaction2);
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
            HardMeterCompletedDateTime = (DateTime)values["HardMeterCompletedDateTime"];
            State = (HardMeterOutState)values["HardMeterOutState"];

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
                transaction[element, "HardMeterCompletedDateTime"] = HardMeterCompletedDateTime;
                transaction[element, "HardMeterOutState"] = State;

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} LogSequence={1}, DateTime={2}, TransactionId={3}, Amount={4}, Reason={5}, State={6}, HardMeterCompletedDateTime={7}]",
                GetType(),
                LogSequence,
                TransactionDateTime.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                Amount,
                Reason,
                State,
                HardMeterCompletedDateTime
                );

            return builder.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var hardMeterOutTransaction = obj as HardMeterOutTransaction;
            return hardMeterOutTransaction != null && Equals(hardMeterOutTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        public override object Clone()
        {
            var copy = new HardMeterOutTransaction(
                BankTransactionId,
                TransactionDateTime,
                Amount,
                Reason)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                TraceId = TraceId,
                State = State,
                HardMeterCompletedDateTime = HardMeterCompletedDateTime
            };

            return copy;
        }

        /// <summary>
        ///     Checks that two HardMeterOutTransaction are the same by value.
        /// </summary>
        /// <param name="hardMeterOutTransaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(HardMeterOutTransaction hardMeterOutTransaction)
        {
            return hardMeterOutTransaction != null &&
                   base.Equals(hardMeterOutTransaction);
        }
    }
}