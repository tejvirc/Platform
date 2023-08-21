namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_GAME_PLAY
    ///     Struct:   GMessageGamePlay
    ///     Response: SMessageGameBonanza
    /// </summary>
    public class GamePlayRequest : Request
    {
        /// <summary>
        /// </summary>
        public GamePlayRequest()
            : base(Command.CmdGamePlay)
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
        public bool ForceCheck;

        /// <summary>
        /// </summary>
        public ushort LinesPlayed;

        /// <summary>
        /// </summary>
        public uint GameMode;

        /// <summary>
        /// </summary>
        public uint RaceTicketSetId;
    }
}