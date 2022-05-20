namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_PLAYER_REQUEST
    ///     Struct:   GMessagePlayerRequest
    ///     Response: SMessagePlayerRequestResponse
    /// </summary>
    public class PlayerIdRequest : Request
    {
        /// <summary>
        /// </summary>
        public PlayerIdRequest()
            : base(Command.CmdPlayerRequest)
        {
        }
    }
}