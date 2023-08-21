namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_PROG_REQUEST
    ///     Struct:   GMessageProgRequest
    ///     Response: SMessageProgressiveInfo
    /// </summary>
    public class ProgressiveInfoRequest : Request
    {
        /// <summary>
        /// </summary>
        public ProgressiveInfoRequest()
            : base(Command.CmdProgRequest)
        {
        }

        /// <summary>
        /// </summary>
        public uint ProgressiveId;
    }
}