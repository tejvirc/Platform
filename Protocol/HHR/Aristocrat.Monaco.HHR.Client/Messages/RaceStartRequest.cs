namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using Data;

    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_RACE_START
    ///     Struct:   GMessageRaceStart
    ///     Response: SMessageGameBonanza (GamePlayResponse)
    /// </summary>
    public class RaceStartRequest : Request
    {
        /// <summary>
        /// </summary>
        public RaceStartRequest()
            : base(Command.CmdRaceStart)
        {
        }

        /// <summary>
        /// </summary>
        public uint GameId;

        /// <summary>
        /// </summary>
        public string PlayerId;

        /// <summary>
        /// </summary>
        public ushort CreditsPlayed;

        /// <summary>
        /// </summary>
        public ushort LinesPlayed;

        /// <summary>
        /// </summary>
        public uint RaceTicketSetId;

        /// <summary>
        /// </summary>
        public uint RaceTicketId;

        /// <summary>
        /// </summary>
        public CRaceInfo RaceInfo;
    }
}