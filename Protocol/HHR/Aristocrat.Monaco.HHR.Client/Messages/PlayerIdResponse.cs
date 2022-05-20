namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_PLAYER_REQUEST_RESPONSE
    ///     Struct:   SMessagePlayerRequestResponse
    ///     Request:  GMessagePlayerRequest
    /// </summary>
    public class PlayerIdResponse : Response
    {
        /// <summary>
        /// </summary>
        public PlayerIdResponse()
            : base(Command.CmdPlayerRequestResponse)
        {
        }

        /// <summary>
        /// </summary>
        public string PlayerId;
    }
}