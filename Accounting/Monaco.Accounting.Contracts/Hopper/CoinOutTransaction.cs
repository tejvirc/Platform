namespace Aristocrat.Monaco.Accounting.Contracts.Hopper
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;
    using Newtonsoft.Json;
    using Transactions;
    using TransferOut;

    /// <summary>
    ///     Definition of the CoinOutTransaction class
    /// </summary>
    [Serializable]
    public sealed class CoinOutTransaction : BaseTransaction, ITransactionConnector, ITransactionContext, IEquatable<CoinOutTransaction>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinOutTransaction" /> class.  This constructor is only used.
        ///     by the transaction framework.
        /// </summary>
        public CoinOutTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinOutTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="cashableAmount">The cashable amount</param>
        /// <param name="reason">The transfer out reason</param>
        public CoinOutTransaction(
            int deviceId,
            DateTime transactionDateTime,
            long cashableAmount,
            TransferOutReason reason = TransferOutReason.CashOut)
            : base(deviceId, transactionDateTime)
        {
            CashableAmount = cashableAmount;
            Reason = reason;
            AssociatedTransactions = Enumerable.Empty<long>();
        }

        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        public long CashableAmount { get; set; }

        /// <summary>
        ///     Gets the reason for the transfer out
        /// </summary>
        public TransferOutReason Reason { get; private set; }

        /// <inheritdoc />
        public override string Name => Localizer.For(CultureFor.Player).GetString(ResourceKeys.CoinOutTransactionName);

        /// <summary>
        ///     Gets the authorized cashable amount
        /// </summary>
        public long AuthorizedCashableAmount { get; set; }

        /// <summary>
        ///     Gets the transferred cashable amount
        /// </summary>
        public long TransferredCashableAmount { get; set; }

        /// <summary>
        ///     Gets the exception
        /// </summary>
        public bool Exception { get; set; }

        /// <summary>
        ///     Gets or sets the associated bank transaction Id
        /// </summary>
        public Guid BankTransactionId { get; set; }

        /// <inheritdoc />
        public IEnumerable<long> AssociatedTransactions { get; set; }

        /// <inheritdoc />
        public Guid TraceId { get; set; }

        /// <inheritdoc />
        public long TransactionAmount => TransferredCashableAmount;

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="coinOutTransaction1">The first transaction</param>
        /// <param name="coinOutTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(CoinOutTransaction coinOutTransaction1, CoinOutTransaction coinOutTransaction2)
        {
            if (ReferenceEquals(coinOutTransaction1, coinOutTransaction2))
            {
                return true;
            }

            if (coinOutTransaction1 is null || coinOutTransaction2 is null)
            {
                return false;
            }

            return coinOutTransaction1.Equals(coinOutTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="coinOutTransaction1">The first transaction</param>
        /// <param name="coinOutTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(CoinOutTransaction coinOutTransaction1, CoinOutTransaction coinOutTransaction2)
        {
            return !(coinOutTransaction1 == coinOutTransaction2);
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            Reason = (TransferOutReason)values["Reason"];
            CashableAmount = (long)values["CashableAmount"];
            AuthorizedCashableAmount = (long)values["AuthorizedCashableAmount"];
            TransferredCashableAmount = (long)values["TransferredCashableAmount"];
            Exception = (bool)values["Exception"];

            var bankTransactionId = values["BankTransactionId"];
            if (bankTransactionId != null)
            {
                BankTransactionId = (Guid)bankTransactionId;
            }

            var associatedTransactions = (string)values["AssociatedTransactions"];
            AssociatedTransactions = !string.IsNullOrEmpty(associatedTransactions)
                ? JsonConvert.DeserializeObject<List<long>>(associatedTransactions)
                : Enumerable.Empty<long>();


            var traceId = values["TraceId"];
            if (traceId != null)
            {
                TraceId = (Guid)traceId;
            }

            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var coinOutTransaction = obj as CoinOutTransaction;
            return coinOutTransaction != null && Equals(coinOutTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);
            using (var transaction = block.StartTransaction())
            {
                transaction[element, "CashableAmount"] = CashableAmount;
                transaction[element, "AuthorizedCashableAmount"] = AuthorizedCashableAmount;
                transaction[element, "TransferredCashableAmount"] = TransferredCashableAmount;
                transaction[element, "Exception"] = Exception;
                transaction[element, "BankTransactionId"] = BankTransactionId;
                transaction[element, "AssociatedTransactions"] =
                    JsonConvert.SerializeObject(AssociatedTransactions, Formatting.None);
                transaction[element, "TraceId"] = TraceId;
                transaction[element, "Reason"] = Reason;

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override object Clone()
        {
            var copy = new CoinOutTransaction(
                DeviceId,
                TransactionDateTime,
                CashableAmount,
                Reason)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                AuthorizedCashableAmount = AuthorizedCashableAmount,
                TransferredCashableAmount = TransferredCashableAmount,
                Exception = Exception,
                BankTransactionId = BankTransactionId,
                TraceId = TraceId
            };

            return copy;
        }

        /// <summary>
        ///     Checks that two CoinOutTransaction are the same by value.
        /// </summary>
        /// <param name="other">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(CoinOutTransaction other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} [DeviceId={1}, LogSequence={2}, DateTime={3}, TransactionId={4}, Cashable={5}, TransferredCash={6}, Reason={7}]",
                GetType(),
                DeviceId,
                LogSequence,
                TransactionDateTime.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                CashableAmount,
                TransferredCashableAmount,
                Reason);

            return builder.ToString();
        }
    }
}
