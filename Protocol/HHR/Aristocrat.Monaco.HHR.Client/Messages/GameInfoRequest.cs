namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_GAME_REQUEST
    ///     Struct:   GMessageGameRequest
    ///     Response: SMessageGameOpen
    /// </summary>
    public class GameInfoRequest : Request
    {
        /// <summary>
        /// </summary>
        public GameInfoRequest()
            : base(Command.CmdGameRequest)
        {
        }

        /// <summary>
        /// </summary>
        public int GameId;
    }
}