namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Application.Contracts.Localization;
    using Common;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;
    using Newtonsoft.Json;
    using Transactions;
    using TransferOut;

    /// <summary>
    ///     VoucherOutTransaction encapsulates and persists the data for a single
    ///     voucher-out transaction.
    /// </summary>
    [Serializable]
    public class VoucherOutTransaction : VoucherBaseTransaction, IAcknowledgeableTransaction, ITransactionContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherOutTransaction" /> class.
        ///     This constructor is only used by the transaction framework.
        /// </summary>
        public VoucherOutTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherOutTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="amount">The currency amount of the voucher</param>
        /// <param name="accountType">The type of credits on the voucher</param>
        /// <param name="barcode">The barcode of the issued voucher</param>
        /// <param name="expiration">Days until expiration of the issued voucher</param>
        /// <param name="manualVerification">Manual verification code.</param>
        public VoucherOutTransaction(
            int deviceId,
            DateTime transactionDateTime,
            long amount,
            AccountType accountType,
            string barcode,
            int expiration,
            string manualVerification)
            : base(deviceId, transactionDateTime, amount, accountType, barcode)
        {
            Expiration = expiration;
            ManualVerification = manualVerification;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherOutTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="amount">The currency amount of the voucher</param>
        /// <param name="accountType">The type of credits on the voucher</param>
        /// <param name="barcode">The barcode of the issued voucher</param>
        /// <param name="expiration">Days until expiration of the issued voucher</param>
        /// <param name="referenceId">The pool id</param>
        /// <param name="manualVerification">Manual verification code.</param>
        public VoucherOutTransaction(
            int deviceId,
            DateTime transactionDateTime,
            long amount,
            AccountType accountType,
            string barcode,
            int expiration,
            int referenceId,
            string manualVerification)
            : base(deviceId, transactionDateTime, amount, accountType, barcode)
        {
            Expiration = expiration;
            ManualVerification = manualVerification;
            ReferenceId = referenceId;
        }

        /// <summary>
        ///     Gets the expiration for the voucher.
        /// </summary>
        public int Expiration { get; private set; }

        /// <summary>
        ///     Gets the voucher sequence for the voucher.
        /// </summary>
        public int VoucherSequence { get; set; }

        /// <summary>
        ///     Gets the barcode for the voucher.
        /// </summary>
        public string ManualVerification { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the voucher has been printed.
        /// </summary>
        public bool VoucherPrinted { get; set; }

        /// <summary>
        ///     Gets or sets the associated bank transaction Id
        /// </summary>
        public Guid BankTransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the reference id for this associated transaction
        /// </summary>
        public int ReferenceId { get; set; }

        /// <summary>
        ///     Gets the human readable name for this transaction type
        /// </summary>
        public override string Name => Localizer.For(CultureFor.Player).GetString(ResourceKeys.VoucherOut);

        /// <summary>
        ///     Gets or sets a value indicating whether the HostAcknowledged for the voucher.
        /// </summary>
        public bool HostAcknowledged { get; set; }

        /// <inheritdoc />
        public long HostSequence { get; set; }

        /// <inheritdoc />
        public Guid TraceId { get; set; }

        /// <inheritdoc />
        public long TransactionAmount => Amount;

        /// <summary>
        ///     Gets or sets the reason for the transfer
        /// </summary>
        public TransferOutReason Reason { get; set; }

        /// <summary>
        ///     Tells the status of Validating host.
        /// </summary>
        public bool HostOnline { get; set; }

        /// <summary>
        ///     Gets or sets ticket data.
        /// </summary>
        public Dictionary<string, string> TicketData { get; set; }

        /// <inheritdoc />
        public override object Clone()
        {
            var copy = new VoucherOutTransaction(
                DeviceId,
                TransactionDateTime,
                Amount,
                TypeOfAccount,
                Barcode,
                Expiration,
                ReferenceId,
                ManualVerification)
            {
                VoucherSequence = VoucherSequence,
                VoucherPrinted = VoucherPrinted,
                HostAcknowledged = HostAcknowledged,
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                BankTransactionId = BankTransactionId,
                HostSequence = HostSequence,
                AssociatedTransactions = AssociatedTransactions.ToList(),
                TraceId = TraceId,
                Reason = Reason,
                HostOnline = HostOnline,
                TicketData = TicketData?.DeepClone(),
                LogDisplayType = LogDisplayType
            };

            return copy;
        }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="voucherOutTransaction1">The first transaction</param>
        /// <param name="voucherOutTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(
            VoucherOutTransaction voucherOutTransaction1,
            VoucherOutTransaction voucherOutTransaction2)
        {
            if (ReferenceEquals(voucherOutTransaction1, voucherOutTransaction2))
            {
                return true;
            }

            if (voucherOutTransaction1 is null || voucherOutTransaction2 is null)
            {
                return false;
            }

            return voucherOutTransaction1.Equals(voucherOutTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="voucherOutTransaction1">The first transaction</param>
        /// <param name="voucherOutTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(
            VoucherOutTransaction voucherOutTransaction1,
            VoucherOutTransaction voucherOutTransaction2)
        {
            return !(voucherOutTransaction1 == voucherOutTransaction2);
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            VoucherSequence = (int)values["VoucherSequence"];
            Expiration = (int)values["Expiration"];

            HostAcknowledged = (bool)values["HostAcknowledged"];
            HostSequence = (long)values["HostSequence"];
            VoucherPrinted = (bool)values["VoucherPrinted"];

            ManualVerification = (string)values["ManualVerification"];

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

            Reason = (TransferOutReason)(values["Reason"] ?? TransferOutReason.CashOut);
            HostOnline = (bool)values["HostOnline"];

            var ticketData = (string)values["TicketDataBlob"];
            TicketData = !string.IsNullOrEmpty(ticketData)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(ticketData)
                : null;

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);

            using (var transaction = block.StartTransaction())
            {
                transaction[element, "VoucherSequence"] = VoucherSequence;
                transaction[element, "Expiration"] = Expiration;
                transaction[element, "HostAcknowledged"] = HostAcknowledged;
                transaction[element, "VoucherPrinted"] = VoucherPrinted;
                transaction[element, "ManualVerification"] = ManualVerification;
                transaction[element, "BankTransactionId"] = BankTransactionId;
                transaction[element, "ReferenceId"] = ReferenceId;
                transaction[element, "HostSequence"] = HostSequence;
                transaction[element, "TraceId"] = TraceId;
                transaction[element, "Reason"] = (int)Reason;
                transaction[element, "HostOnline"] = HostOnline;
                transaction[element, "TicketDataBlob"] = JsonConvert.SerializeObject(TicketData);
                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"{GetType()} [DeviceId={DeviceId}, LogSequence={LogSequence}, ");
            builder.Append(
                $"DateTime={TransactionDateTime.ToString(CultureInfo.InvariantCulture)}, TransactionId={TransactionId}, Amount={Amount},");
            builder.Append(
                $" TypeOfAccount={TypeOfAccount}, Barcode={Barcode}, Expiration = {Expiration}, HostAcknowledged = {HostAcknowledged},");
            builder.Append(
                $" VoucherPrinted = {VoucherPrinted}, VoucherSequence = {VoucherSequence}, ManualVerification = {ManualVerification},");
            builder.Append($" Reason = {Reason}]");

            return builder.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var voucherOutTransaction = obj as VoucherOutTransaction;
            return voucherOutTransaction != null && Equals(voucherOutTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Barcode.GetHashCode();
        }

        /// <summary>
        ///     Checks that two VoucherOutTransaction are the same by value.
        /// </summary>
        /// <param name="voucherOutTransaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(VoucherOutTransaction voucherOutTransaction)
        {
            return voucherOutTransaction != null &&
                   Barcode == voucherOutTransaction.Barcode &&
                   base.Equals(voucherOutTransaction);
        }
    }
}