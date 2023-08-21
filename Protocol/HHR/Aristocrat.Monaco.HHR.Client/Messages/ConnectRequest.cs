namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_CONNECT
    ///     Struct:   MessageConnect
    ///     Response: EmptyResponse
    /// </summary>
    public class ConnectRequest : Request
    {
        /// <summary>
        /// </summary>
        public ConnectRequest()
            : base(Command.CmdConnect)
        {
        }
    }
}