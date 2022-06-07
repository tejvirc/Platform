namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
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
    ///     HandpayTransaction encapsulates and persists the data for a single hand pay transaction.
    /// </summary>
    [Serializable]
    public class HandpayTransaction :
        BaseTransaction, IAcknowledgeableTransaction, ITransactionConnector, ITransactionContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayTransaction" /> class.
        /// </summary>
        public HandpayTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="cashableAmount">The cashable amount requiring a handpay</param>
        /// <param name="promoAmount">The promotional amount requiring a handpay</param>
        /// <param name="nonCashAmount">The non-cashable amount requiring a handpay</param>
        /// <param name="wagerAmount">Wager responsible for handpay</param>
        /// <param name="type">The handpay type</param>
        /// <param name="printTicket">true if the associated ticket should be printed</param>
        /// <param name="transactionId">The current transactionId</param>
        public HandpayTransaction(
            int deviceId,
            DateTime transactionDateTime,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            long wagerAmount,
            HandpayType type,
            bool printTicket,
            Guid transactionId)
            : base(deviceId, transactionDateTime)
        {
            CashableAmount = cashableAmount;
            PromoAmount = promoAmount;
            NonCashAmount = nonCashAmount;
            WagerAmount = wagerAmount;
            HandpayType = type;
            PrintTicket = printTicket;
            BankTransactionId = transactionId;
            AssociatedTransactions = Enumerable.Empty<long>();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the receipt was printed
        /// </summary>
        public bool Printed { get; set; }

        /// <summary>
        ///     Gets or sets the expiration for the handpay receipt.
        /// </summary>
        public int Expiration { get; set; }

        /// <summary>
        ///     Gets the cashable amount
        /// </summary>
        public long CashableAmount { get; private set; }

        /// <summary>
        ///     Gets the promo amount
        /// </summary>
        public long PromoAmount { get; private set; }

        /// <summary>
        ///     Gets the non-cashable amount
        /// </summary>
        public long NonCashAmount { get; private set; }

        /// <summary>
        ///     Gets the wager amount
        /// </summary>
        public long WagerAmount { get; private set; }

        /// <summary>
        ///     Gets the handpay type
        /// </summary>
        public HandpayType HandpayType { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not a ticket can be printed
        /// </summary>
        public bool PrintTicket { get; set; }

        /// <summary>
        ///     Gets whether or not the backend validated the handpay
        /// </summary>
        public bool Validated => !string.IsNullOrEmpty(Barcode);

        /// <summary>
        ///     Gets the key off type
        /// </summary>
        public KeyOffType KeyOffType { get; set; }

        /// <summary>
        ///     Gets the key off cashable amount
        /// </summary>
        public long KeyOffCashableAmount { get; set; }

        /// <summary>
        ///     Gets the key off promo amount
        /// </summary>
        public long KeyOffPromoAmount { get; set; }

        /// <summary>
        ///     Gets the key off non-cash amount
        /// </summary>
        public long KeyOffNonCashAmount { get; set; }

        /// <summary>
        ///     Gets the key off date/time
        /// </summary>
        public DateTime KeyOffDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the state
        /// </summary>
        public HandpayState State { get; set; }

        /// <summary>
        ///     Gets or sets the associated bank transaction Id
        /// </summary>
        public Guid BankTransactionId { get; private set; }

        /// <summary>
        ///     Gets or sets the barcode
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the host acknowledged the hand pay request.
        /// </summary>
        public bool RequestAcknowledged { get; set; }

        /// <summary>
        ///     Gets the sequence for the receipt.
        /// </summary>
        public int ReceiptSequence { get; set; }

        /// <summary>
        ///     Gets or set a value indicating whether transaction was read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        ///     Tells the status of Validating host.
        /// </summary>
        public bool HostOnline { get; set; }

        /// <summary>
        ///     Gets or sets ticket data.
        /// </summary>
        public Dictionary<string, string> TicketData { get; set; }

        /// <summary>
        ///     Check if the TransactionName has been Overriden
        /// </summary>
        public bool TransactionNameOverride { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Get the Name of the Transaction
        /// </summary>
        public override string Name => GetTransactionName();

        /// <inheritdoc />
        public long HostSequence { get; set; }

        /// <summary>
        ///     Gets or sets the reason for the transfer
        /// </summary>
        public TransferOutReason Reason { get; set; }

        /// <inheritdoc />
        public override object Clone()
        {
            return new HandpayTransaction(
                DeviceId,
                TransactionDateTime,
                CashableAmount,
                PromoAmount,
                NonCashAmount,
                WagerAmount,
                HandpayType,
                PrintTicket,
                BankTransactionId)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                Printed = Printed,
                HandpayType = HandpayType,
                KeyOffType = KeyOffType,
                KeyOffCashableAmount = KeyOffCashableAmount,
                KeyOffPromoAmount = KeyOffPromoAmount,
                WagerAmount = WagerAmount,
                KeyOffNonCashAmount = KeyOffNonCashAmount,
                KeyOffDateTime = KeyOffDateTime,
                State = State,
                Barcode = Barcode,
                RequestAcknowledged = RequestAcknowledged,
                ReceiptSequence = ReceiptSequence,
                HostSequence = HostSequence,
                AssociatedTransactions = AssociatedTransactions.ToList(),
                TraceId = TraceId,
                Read = Read,
                Expiration = Expiration,
                HostOnline = HostOnline,
                TicketData = TicketData?.DeepClone(),
                TransactionNameOverride = TransactionNameOverride,
                Reason = Reason
            };
        }

        /// <inheritdoc />
        public IEnumerable<long> AssociatedTransactions { get; set; }

        /// <inheritdoc />
        public Guid TraceId { get; set; }

        /// <inheritdoc />
        public long TransactionAmount => KeyOffCashableAmount + KeyOffPromoAmount + KeyOffNonCashAmount;

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="canceledTransaction1">The first transaction</param>
        /// <param name="canceledTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(
            HandpayTransaction canceledTransaction1,
            HandpayTransaction canceledTransaction2)
        {
            if (ReferenceEquals(canceledTransaction1, canceledTransaction2))
            {
                return true;
            }

            if (canceledTransaction1 is null || canceledTransaction2 is null)
            {
                return false;
            }

            return canceledTransaction1.Equals(canceledTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="canceledTransaction1">The first transaction</param>
        /// <param name="canceledTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(
            HandpayTransaction canceledTransaction1,
            HandpayTransaction canceledTransaction2)
        {
            return !(canceledTransaction1 == canceledTransaction2);
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            // get the base persistence members
            var success = base.SetData(values);

            if (success)
            {
                Printed = (bool)values["Printed"];
                CashableAmount = (long)values["Amount"];
                PromoAmount = (long)values["PromoAmount"];
                NonCashAmount = (long)values["NonCashAmount"];
                HandpayType = (HandpayType)values["HandpayType"];
                KeyOffType = (KeyOffType)values["KeyOffType"];
                KeyOffCashableAmount = (long)values["KeyOffCashableAmount"];
                KeyOffPromoAmount = (long)values["KeyOffPromoAmount"];
                KeyOffNonCashAmount = (long)values["KeyOffNonCashAmount"];
                WagerAmount = (long)values["WagerAmount"];
                KeyOffDateTime = (DateTime)values["KeyOffDateTime"];
                State = (HandpayState)values["HandpayState"];
                PrintTicket = (bool)values["PrintTicket"];
                Barcode = (string)values["Barcode"];
                RequestAcknowledged = (bool)values["RequestAcknowledged"];
                HostSequence = (long)values["HostSequence"];
                ReceiptSequence = (int)values["ReceiptSequence"];
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

                Read = (bool)values["Read"];
                Expiration = (int)values["Expiration"];
                HostOnline = (bool)values["HostOnline"];

                var ticketData = (string)values["TicketDataBlob"];
                TicketData = !string.IsNullOrEmpty(ticketData)
                    ? JsonConvert.DeserializeObject<Dictionary<string, string>>(ticketData)
                    : null;
            }

            return success;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            // save the base persistence also
            base.SetPersistence(block, element);

            using (var transaction = block.StartTransaction())
            {
                transaction[element, "Printed"] = Printed;
                transaction[element, "Amount"] = CashableAmount;
                transaction[element, "PromoAmount"] = PromoAmount;
                transaction[element, "NonCashAmount"] = NonCashAmount;
                transaction[element, "WagerAmount"] = WagerAmount;
                transaction[element, "HandpayType"] = HandpayType;
                transaction[element, "KeyOffType"] = KeyOffType;
                transaction[element, "KeyOffCashableAmount"] = KeyOffCashableAmount;
                transaction[element, "KeyOffPromoAmount"] = KeyOffPromoAmount;
                transaction[element, "KeyOffNonCashAmount"] = KeyOffNonCashAmount;
                transaction[element, "KeyOffDateTime"] = KeyOffDateTime;
                transaction[element, "HandpayState"] = State;
                transaction[element, "PrintTicket"] = PrintTicket;
                transaction[element, "BankTransactionId"] = BankTransactionId;
                transaction[element, "Barcode"] = Barcode;
                transaction[element, "RequestAcknowledged"] = RequestAcknowledged;
                transaction[element, "HostSequence"] = HostSequence;
                transaction[element, "ReceiptSequence"] = ReceiptSequence;
                transaction[element, "AssociatedTransactions"] =
                    JsonConvert.SerializeObject(AssociatedTransactions, Formatting.None);
                transaction[element, "TraceId"] = TraceId;
                transaction[element, "Read"] = Read;
                transaction[element, "Expiration"] = Expiration;
                transaction[element, "HostOnline"] = HostOnline;
                transaction[element, "TicketDataBlob"] = JsonConvert.SerializeObject(TicketData);
                transaction[element, "Reason"] = Reason;

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{GetType()} [DeviceId={DeviceId}, LogSequence={LogSequence}, DateTime={TransactionDateTime.ToString(CultureInfo.InvariantCulture)} TransactionId={TransactionId} Cashable={CashableAmount} Promo={PromoAmount} NonCash={NonCashAmount} Wager={WagerAmount} HandpayType={HandpayType} PrintTicket={PrintTicket} BankTransactionId={BankTransactionId} Printed={Printed} KeyOffType={KeyOffType} KeyOffCashableAmount={KeyOffCashableAmount} KeyOffPromoAmount={KeyOffPromoAmount} KeyOffNonCashAmount={KeyOffNonCashAmount} KeyOffDateTime={KeyOffDateTime.ToString(CultureInfo.InvariantCulture)} State={State} Barcode={Barcode} RequestAcknowledged={RequestAcknowledged} ReceiptSequence={ReceiptSequence} HostSequence={HostSequence} AssociatedTransactions={GetAssociatedTransactions()} TraceId={TraceId} Read={Read} Expiration={Expiration} HostOnline={HostOnline}";

            string GetAssociatedTransactions()
            {
                var result = new StringBuilder();
                foreach (var transaction in AssociatedTransactions)
                {
                    result.Append($"{transaction} ");
                }

                return result.ToString();
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as HandpayTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (TransactionId, TransactionDateTime).GetHashCode();
        }

        /// <summary>
        ///     Checks whether this handpay is supposed to be paid to the credit meter
        /// </summary>
        /// <returns>True when KeyOffType is LocalCredit or RemoteCredit</returns>
        public bool IsCreditType()
        {
            return KeyOffType == KeyOffType.LocalCredit || KeyOffType == KeyOffType.RemoteCredit;
        }

        /// <summary>
        ///     Checks that two HandpayTransaction are the same by value.
        /// </summary>
        /// <param name="transaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(HandpayTransaction transaction)
        {
            return base.Equals(transaction) &&
                   TransactionId == transaction.TransactionId;
        }

        private string GetTransactionName()
        {
            if (HandpayType == HandpayType.GameWin && TransactionNameOverride)
            {
                return Localizer.For(CultureFor.Player)
                    .GetString(ResourceKeys.JackpotHandpayTransactionName);
            }

            // Default Value
            return Localizer.For(CultureFor.Player).GetString(ResourceKeys.Handpay);
        }
    }
}