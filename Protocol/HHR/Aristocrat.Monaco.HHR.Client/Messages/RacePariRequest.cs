namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_RACE_PARI_REQ
    ///     Struct:   GMessageRacePariRequest
    ///     Response: GMessageRacePariResponse
    /// </summary>
    public class RacePariRequest : Request
    {
        /// <summary>
        /// </summary>
        public RacePariRequest()
            : base(Command.CmdRacePariReq)
        {
        }

        /// <summary>
        /// </summary>
        public uint GameId;

        /// <summary>
        /// </summary>
        public uint CreditsPlayed;

        /// <summary>
        /// </summary>
        public uint LinesPlayed;
    }
}