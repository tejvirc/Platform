namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_TRANSACTION
    ///     Struct:   MessageTransaction
    ///     Response: MessageCloseTran
    /// </summary>
    public class TransactionRequest : Request
    {
        /// <summary>
        ///     Unique transaction ID for this request
        /// </summary>
        public uint TransactionId;

        /// <summary>
        ///     Current player id
        /// </summary>
        public string PlayerId;

        /// <summary>
        ///     Transaction type for this request
        /// </summary>
        public CommandTransactionType TransactionType;

        /// <summary>
        ///     Amount credited
        /// </summary>
        public uint Credit;

        /// <summary>
        ///     Amount debited
        /// </summary>
        public uint Debit;

        /// <summary>
        ///     Current cash balance = cashable + promo
        /// </summary>
        public uint CashBalance;

        /// <summary>
        ///     Current non-cash balance
        /// </summary>
        public uint NonCashBalance;

        /// <summary>
        ///     Current Game Id
        /// </summary>
        public uint GameMapId;

        /// <summary>
        /// </summary>
        public uint Flags;

        /// <summary>
        ///     Denom
        /// </summary>
        public uint Denomination;

        /// <summary>
        ///     Handpay type, progressive or non-progressive
        /// </summary>
        public uint HandpayType;

        /// <summary>
        ///     Last game play time
        /// </summary>
        public uint LastGamePlayTime;

        /// <summary>
        ///     Constructs instance of <see cref="TransactionRequest" />
        /// </summary>
        public TransactionRequest()
            : base(Command.CmdTransaction)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"MsgId={SequenceId}, Cmd={Command}, TransType={TransactionType}";
        }
    }
}