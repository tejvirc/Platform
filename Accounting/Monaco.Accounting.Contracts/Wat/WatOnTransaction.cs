namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;
    using ProtoBuf;
    using Transactions;
    using Wat;

    /// <summary>
    ///     Definition of the WatOnTransaction class.  This transaction provides and persists data for
    ///     a single WAT On transaction.
    /// </summary>
    [ProtoContract]
    public class WatOnTransaction : BaseTransaction, ITransactionTotal
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WatOnTransaction" /> class.  This constructor is only used.
        ///     by the transaction framework.
        /// </summary>
        public WatOnTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WatOnTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="cashableAmount">The cashable amount</param>
        /// <param name="promoAmount">The promotional amount</param>
        /// <param name="nonCashAmount">The non-cashable amount</param>
        /// <param name="allowReducedAmounts">whether or not we allow reduced amounts</param>
        /// <param name="requestId">The transaction id provided by the WAT host</param>
        public WatOnTransaction(
            int deviceId,
            DateTime transactionDateTime,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            bool allowReducedAmounts,
            string requestId)
            : base(deviceId, transactionDateTime)
        {
            CashableAmount = cashableAmount;
            PromoAmount = promoAmount;
            NonCashAmount = nonCashAmount;
            AllowReducedAmounts = allowReducedAmounts;
            RequestId = requestId;
        }

        /// <summary>
        ///     Gets the total authorized amount
        /// </summary>
        public long AuthorizedAmount => AuthorizedCashableAmount + AuthorizedPromoAmount + AuthorizedNonCashAmount;

        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        [ProtoMember(1)]
        public long CashableAmount { get; set; }

        /// <summary>
        ///     Gets the promo amount
        /// </summary>
        [ProtoMember(2)]
        public long PromoAmount { get; set; }

        /// <summary>
        ///     Gets the non-cashable amount
        /// </summary>
        [ProtoMember(3)]
        public long NonCashAmount { get; set; }

        /// <summary>
        ///     Gets or sets whether or not reduced amounts are allowed
        /// </summary>
        [ProtoMember(4)]
        public bool AllowReducedAmounts { get; private set; }

        /// <summary>
        ///     Gets or sets the transaction identifier provided by the WAT host.
        /// </summary>
        [ProtoMember(5)]
        public string RequestId { get; private set; }

        /// <summary>
        ///     Gets the authorized cashable amount
        /// </summary>
        [ProtoMember(6)]
        public long AuthorizedCashableAmount { get; set; }

        /// <summary>
        ///     Gets the authorized promo amount
        /// </summary>
        [ProtoMember(7)]
        public long AuthorizedPromoAmount { get; set; }

        /// <summary>
        ///     Gets the authorized non-cashable amount
        /// </summary>
        [ProtoMember(8)]
        public long AuthorizedNonCashAmount { get; set; }

        /// <summary>
        ///     Gets the host exception
        /// </summary>
        [ProtoMember(9)]
        public int HostException { get; set; }

        /// <summary>
        ///     Gets the EGM exception
        /// </summary>
        [ProtoMember(10)]
        public int EgmException { get; set; }

        /// <summary>
        ///     Gets the transferred cashable amount
        /// </summary>
        [ProtoMember(11)]
        public long TransferredCashableAmount { get; set; }

        /// <summary>
        ///     Gets the transferred promo amount
        /// </summary>
        [ProtoMember(12)]
        public long TransferredPromoAmount { get; set; }

        /// <summary>
        ///     Gets the transferred non-cashable amount
        /// </summary>
        [ProtoMember(13)]
        public long TransferredNonCashAmount { get; set; }

        /// <summary>
        ///     Gets the status of the transaction
        /// </summary>
        [ProtoMember(14)]
        public WatStatus Status { get; set; }

        /// <summary>
        ///     Gets or sets the associated bank transaction Id
        /// </summary>
        [ProtoMember(15)]
        public Guid BankTransactionId { get; set; }

        /// <summary>
        ///     Gets a value indicating whether or not the provider created the bank transaction id
        /// </summary>
        /// <remarks>
        ///     This is really only meaningful while the transaction is active
        /// </remarks>
        [ProtoMember(16)]
        public bool OwnsBankTransaction { get; set; }

        /// <inheritdoc />
        public override string Name => Localizer.For(CultureFor.Player).GetString(ResourceKeys.WatOnTransactionName);

        /// <inheritdoc />
        /// >
        public long TransactionAmount => TransferredCashableAmount + TransferredPromoAmount + TransferredNonCashAmount;

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="watOnTransaction1">The first transaction</param>
        /// <param name="watOnTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(WatOnTransaction watOnTransaction1, WatOnTransaction watOnTransaction2)
        {
            if (ReferenceEquals(watOnTransaction1, watOnTransaction2))
            {
                return true;
            }

            if (watOnTransaction1 is null || watOnTransaction2 is null)
            {
                return false;
            }

            return watOnTransaction1.Equals(watOnTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="watOnTransaction1">The first transaction</param>
        /// <param name="watOnTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(WatOnTransaction watOnTransaction1, WatOnTransaction watOnTransaction2)
        {
            return !(watOnTransaction1 == watOnTransaction2);
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
            AuthorizedCashableAmount = (long)values["AuthorizedCashableAmount"];
            AuthorizedPromoAmount = (long)values["AuthorizedPromoAmount"];
            AuthorizedNonCashAmount = (long)values["AuthorizedNonCashAmount"];
            HostException = (int)values["HostException"];
            EgmException = (int)values["EgmException"];
            RequestId = values["RequestId"].ToString().TrimEnd('\0');
            TransferredCashableAmount = (long)values["TransferredCashableAmount"];
            TransferredPromoAmount = (long)values["TransferredPromoAmount"];
            TransferredNonCashAmount = (long)values["TransferredNonCashAmount"];
            Status = (WatStatus)values["Status"];
            OwnsBankTransaction = (bool)values["OwnsBankTransaction"];

            var bankTransId = values["BankTransactionId"];
            if (bankTransId != null)
            {
                BankTransactionId = (Guid)bankTransId;
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
                transaction[element, "RequestId"] = RequestId;
                transaction[element, "AuthorizedCashableAmount"] = AuthorizedCashableAmount;
                transaction[element, "AuthorizedPromoAmount"] = AuthorizedPromoAmount;
                transaction[element, "AuthorizedNonCashAmount"] = AuthorizedNonCashAmount;
                transaction[element, "HostException"] = HostException;
                transaction[element, "EgmException"] = EgmException;
                transaction[element, "TransferredCashableAmount"] = TransferredCashableAmount;
                transaction[element, "TransferredPromoAmount"] = TransferredPromoAmount;
                transaction[element, "TransferredNonCashAmount"] = TransferredNonCashAmount;
                transaction[element, "Status"] = Status;
                transaction[element, "BankTransactionId"] = BankTransactionId;
                transaction[element, "OwnsBankTransaction"] = OwnsBankTransaction;

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} [DeviceId={1}, LogSequence={2}, DateTime={3}, TransactionId={4}, , Cashable={5}, Promo={6}, NonCash={7}, AllowReducedAmounts={8}, RequestId={9}]",
                GetType(),
                DeviceId,
                LogSequence,
                TransactionDateTime.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                CashableAmount,
                PromoAmount,
                NonCashAmount,
                AllowReducedAmounts,
                RequestId);

            return builder.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var watOnTransaction = obj as WatOnTransaction;
            return watOnTransaction != null && Equals(watOnTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ AllowReducedAmounts.GetHashCode() ^ RequestId.GetHashCode();
        }

        /// <inheritdoc />
        public override object Clone()
        {
            var copy = new WatOnTransaction(
                DeviceId,
                TransactionDateTime,
                CashableAmount,
                PromoAmount,
                NonCashAmount,
                AllowReducedAmounts,
                RequestId)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                AuthorizedCashableAmount = AuthorizedCashableAmount,
                AuthorizedPromoAmount = AuthorizedPromoAmount,
                AuthorizedNonCashAmount = AuthorizedNonCashAmount,
                HostException = HostException,
                EgmException = EgmException,
                TransferredCashableAmount = TransferredCashableAmount,
                TransferredPromoAmount = TransferredPromoAmount,
                TransferredNonCashAmount = TransferredNonCashAmount,
                Status = Status,
                AllowReducedAmounts = AllowReducedAmounts,
                RequestId = RequestId,
                BankTransactionId = BankTransactionId
            };

            return copy;
        }

        /// <summary>
        ///     Checks that two WatOnTransaction are the same by value.
        /// </summary>
        /// <param name="watOnTransaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(WatOnTransaction watOnTransaction)
        {
            return watOnTransaction != null &&
                   RequestId == watOnTransaction.RequestId &&
                   AllowReducedAmounts == watOnTransaction.AllowReducedAmounts &&
                   base.Equals(watOnTransaction);
        }
    }
}