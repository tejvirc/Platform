namespace Aristocrat.Monaco.Accounting.Contracts.Wat
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
    ///     Definition of the WatTransaction class
    /// </summary>
    public class WatTransaction : BaseTransaction, ITransactionConnector, ITransactionContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WatTransaction" /> class.  This constructor is only used.
        ///     by the transaction framework.
        /// </summary>
        public WatTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WatTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="cashableAmount">The cashable amount requiring a handpay</param>
        /// <param name="promoAmount">The promotional amount requiring a handpay</param>
        /// <param name="nonCashAmount">The non-cashable amount requiring a handpay</param>
        /// <param name="allowReducedAmounts">
        ///     Indicates whether the host is permitted to subsequently reduce the amounts of the
        ///     transfer
        /// </param>
        /// <param name="requestId">The transaction id provided by the WAT host.  0 (zero) if initiated by the EGM</param>
        /// <param name="reason">The transfer out reason</param>
        public WatTransaction(
            int deviceId,
            DateTime transactionDateTime,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            bool allowReducedAmounts,
            string requestId,
            TransferOutReason reason = TransferOutReason.CashOut)
            : base(deviceId, transactionDateTime)
        {
            CashableAmount = cashableAmount;
            PromoAmount = promoAmount;
            NonCashAmount = nonCashAmount;
            RequestId = requestId;
            Reason = reason;
            AllowReducedAmounts = allowReducedAmounts;
            AssociatedTransactions = Enumerable.Empty<long>();
        }

        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        public long CashableAmount { get; set; }

        /// <summary>
        ///     Gets the promo amount
        /// </summary>
        public long PromoAmount { get; set; }

        /// <summary>
        ///     Gets the non-cashable amount
        /// </summary>
        public long NonCashAmount { get; set; }

        /// <summary>
        ///     Gets or sets the transaction identifier provided by the WAT host.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        ///     Gets the reason for the transfer out
        /// </summary>
        public TransferOutReason Reason { get; private set; }

        /// <inheritdoc />
        public override string Name => Localizer.For(CultureFor.Player).GetString(ResourceKeys.WatOffTransactionName);

        /// <summary>
        ///     Gets a value indicating who initiated the transfer
        /// </summary>
        public WatDirection Direction =>
            // Only use null or empty string for EgmInitiated.  0 is not a valid G2S transaction id but it is valid for SAS.
            string.IsNullOrEmpty(RequestId)
                ? WatDirection.EgmInitiated
                : WatDirection.HostInitiated;

        /// <summary>
        ///     Gets a value indicating whether amounts can be reduced
        /// </summary>
        public bool AllowReducedAmounts { get; private set; }

        /// <summary>
        ///     Gets the Payment method for transfers from the host to the EGM
        /// </summary>
        public WatPayMethod PayMethod { get; set; }

        /// <summary>
        ///     Gets the authorized cashable amount
        /// </summary>
        public long AuthorizedCashableAmount { get; set; }

        /// <summary>
        ///     Gets the authorized promo amount
        /// </summary>
        public long AuthorizedPromoAmount { get; set; }

        /// <summary>
        ///     Gets the authorized non-cashable amount
        /// </summary>
        public long AuthorizedNonCashAmount { get; set; }

        /// <summary>
        ///     Gets the host exception
        /// </summary>
        public int HostException { get; set; }

        /// <summary>
        ///     Gets the transferred cashable amount
        /// </summary>
        public long TransferredCashableAmount { get; set; }

        /// <summary>
        ///     Gets the transferred promo amount
        /// </summary>
        public long TransferredPromoAmount { get; set; }

        /// <summary>
        ///     Gets the transferred non-cashable amount
        /// </summary>
        public long TransferredNonCashAmount { get; set; }

        /// <summary>
        ///     Gets the EGM exception
        /// </summary>
        public int EgmException { get; set; }

        /// <summary>
        ///     Gets the status of the transaction
        /// </summary>
        public WatStatus Status { get; set; }

        /// <summary>
        ///     Gets or sets the associated bank transaction Id
        /// </summary>
        public Guid BankTransactionId { get; set; }

        /// <summary>
        ///     Gets a value indicating whether or not the provider created the bank transaction id
        /// </summary>
        /// <remarks>
        ///     This is really only meaningful while the transaction is active
        /// </remarks>
        public bool OwnsBankTransaction { get; set; }

        /// <inheritdoc />
        public IEnumerable<long> AssociatedTransactions { get; set; }

        /// <inheritdoc />
        public Guid TraceId { get; set; }

        /// <inheritdoc />
        public long TransactionAmount => TransferredCashableAmount + TransferredPromoAmount + TransferredNonCashAmount;

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="watOffTransaction1">The first transaction</param>
        /// <param name="watOffTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(WatTransaction watOffTransaction1, WatTransaction watOffTransaction2)
        {
            if (ReferenceEquals(watOffTransaction1, watOffTransaction2))
            {
                return true;
            }

            if (watOffTransaction1 is null || watOffTransaction2 is null)
            {
                return false;
            }

            return watOffTransaction1.Equals(watOffTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="watOffTransaction1">The first transaction</param>
        /// <param name="watOffTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(WatTransaction watOffTransaction1, WatTransaction watOffTransaction2)
        {
            return !(watOffTransaction1 == watOffTransaction2);
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            CashableAmount = (long)values["CashableAmount"];
            PromoAmount = (long)values["PromoAmount"];
            NonCashAmount = (long)values["NonCashAmount"];
            AllowReducedAmounts = (bool)values["AllowReducedAmounts"];
            PayMethod = (WatPayMethod)values["PayMethod"];
            RequestId = values["RequestId"].ToString().TrimEnd('\0');
            AuthorizedCashableAmount = (long)values["AuthorizedCashableAmount"];
            AuthorizedPromoAmount = (long)values["AuthorizedPromoAmount"];
            AuthorizedNonCashAmount = (long)values["AuthorizedNonCashAmount"];
            HostException = (int)values["HostException"];
            TransferredCashableAmount = (long)values["TransferredCashableAmount"];
            TransferredPromoAmount = (long)values["TransferredPromoAmount"];
            TransferredNonCashAmount = (long)values["TransferredNonCashAmount"];
            EgmException = (int)values["EgmException"];
            Status = (WatStatus)values["Status"];
            OwnsBankTransaction = (bool)values["OwnsBankTransaction"];
            Reason = (TransferOutReason)values["Reason"];

            var associated = (string)values["AssociatedTransactions"];
            AssociatedTransactions = !string.IsNullOrEmpty(associated)
                ? JsonConvert.DeserializeObject<List<long>>(associated)
                : Enumerable.Empty<long>();

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
                transaction[element, "CashableAmount"] = CashableAmount;
                transaction[element, "PromoAmount"] = PromoAmount;
                transaction[element, "NonCashAmount"] = NonCashAmount;
                transaction[element, "AllowReducedAmounts"] = AllowReducedAmounts;
                transaction[element, "PayMethod"] = PayMethod;
                transaction[element, "RequestId"] = RequestId;
                transaction[element, "AuthorizedCashableAmount"] = AuthorizedCashableAmount;
                transaction[element, "AuthorizedPromoAmount"] = AuthorizedPromoAmount;
                transaction[element, "AuthorizedNonCashAmount"] = AuthorizedNonCashAmount;
                transaction[element, "HostException"] = HostException;
                transaction[element, "TransferredCashableAmount"] = TransferredCashableAmount;
                transaction[element, "TransferredPromoAmount"] = TransferredPromoAmount;
                transaction[element, "TransferredNonCashAmount"] = TransferredNonCashAmount;
                transaction[element, "EgmException"] = EgmException;
                transaction[element, "Status"] = Status;
                transaction[element, "BankTransactionId"] = BankTransactionId;
                transaction[element, "OwnsBankTransaction"] = OwnsBankTransaction;
                transaction[element, "AssociatedTransactions"] =
                    JsonConvert.SerializeObject(AssociatedTransactions, Formatting.None);
                transaction[element, "TraceId"] = TraceId;
                transaction[element, "Reason"] = Reason;

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} [DeviceId={1}, LogSequence={2}, DateTime={3}, TransactionId={4}, Cashable={5}, Promo={6}, NonCash={7}, AllowReducedAmounts={8}, RequestId={9}, Status={10}, Reason={11}]",
                GetType(),
                DeviceId,
                LogSequence,
                TransactionDateTime.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                CashableAmount,
                PromoAmount,
                NonCashAmount,
                AllowReducedAmounts,
                RequestId,
                Status,
                Reason);

            return builder.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var watOffTransaction = obj as WatTransaction;
            return watOffTransaction != null && Equals(watOffTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ RequestId.GetHashCode();
        }

        /// <inheritdoc />
        public override object Clone()
        {
            var copy = new WatTransaction(
                DeviceId,
                TransactionDateTime,
                CashableAmount,
                PromoAmount,
                NonCashAmount,
                AllowReducedAmounts,
                RequestId,
                Reason)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                AuthorizedCashableAmount = AuthorizedCashableAmount,
                AuthorizedPromoAmount = AuthorizedPromoAmount,
                AuthorizedNonCashAmount = AuthorizedNonCashAmount,
                HostException = HostException,
                TransferredCashableAmount = TransferredCashableAmount,
                TransferredPromoAmount = TransferredPromoAmount,
                TransferredNonCashAmount = TransferredNonCashAmount,
                EgmException = EgmException,
                Status = Status,
                AllowReducedAmounts = AllowReducedAmounts,
                PayMethod = PayMethod,
                RequestId = RequestId,
                BankTransactionId = BankTransactionId,
                TraceId = TraceId
            };

            return copy;
        }

        /// <summary>
        ///     Checks that two WatOffTransaction are the same by value.
        /// </summary>
        /// <param name="watOffTransaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(WatTransaction watOffTransaction)
        {
            return watOffTransaction != null &&
                   RequestId == watOffTransaction.RequestId &&
                   base.Equals(watOffTransaction);
        }
    }
}