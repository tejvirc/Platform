namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_HEARTBEAT_CLOSE_TRAN
    ///     Struct:   MessageCloseTranHeartbeat
    ///     Request:  MessageHeartbeat
    /// </summary>
    public class HeartBeatResponse : Response
    {
        /// <summary>
        /// </summary>
        public HeartBeatResponse()
            : base(Command.CmdHeartbeatCloseTran)
        {
        }
    }
}