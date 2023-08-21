namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_PROG_INFO
    ///     Struct:   SMessageProgressiveInfo
    ///     Request:  GMessageProgRequest
    /// </summary>
    public class ProgressiveInfoResponse : Response
    {
        /// <summary>
        /// </summary>
        public ProgressiveInfoResponse()
            : base(Command.CmdProgInfo)
        {
        }

        /// <summary>
        /// </summary>
        public uint ProgressiveId;

        /// <summary>
        /// </summary>
        public uint ProgLevel;

        /// <summary>
        /// </summary>
        public uint ProgCurrentValue;

        /// <summary>
        /// </summary>
        public uint ProgResetValue;

        /// <summary>
        /// </summary>
        public uint ProgContribPercent;

        /// <summary>
        /// </summary>
        public uint ProgCreditsBet;
    }
}