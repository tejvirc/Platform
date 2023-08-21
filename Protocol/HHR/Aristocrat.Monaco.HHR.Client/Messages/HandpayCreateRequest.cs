namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_HANDPAY_CREATE
    ///     Struct:   GMessageCreateHandPayItem
    ///     Response: MessageCloseTran
    /// </summary>
    public class HandpayCreateRequest : Request
    {
        /// <summary>
        /// </summary>
        public HandpayCreateRequest()
            : base(Command.CmdHandpayCreate)
        {
        }

        /// <summary>
        /// </summary>
        public uint TransactionId;

        /// <summary>
        /// </summary>
        public uint HandpayType;

        /// <summary>
        /// </summary>
        public uint Amount;

        /// <summary>
        /// </summary>
        public uint Denomination;

        /// <summary>
        /// </summary>
        public uint GameWin;

        /// <summary>
        /// </summary>
        public uint ProgWin;

        /// <summary>
        /// </summary>
        public uint LastWager;

        /// <summary>
        /// </summary>
        public string PlayerId;

        /// <summary>
        /// </summary>
        public uint GameMapId;

        /// <summary>
        /// </summary>
        public uint LastGamePlayTime;
    }
}