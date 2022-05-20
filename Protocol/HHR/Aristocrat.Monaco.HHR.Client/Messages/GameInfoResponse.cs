// ReSharper disable UnusedMember.Global
namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using Data;

    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_GAME_OPEN
    ///     Struct:   SMessageGameOpen
    ///     Request:  GMessageGameRequest
    /// </summary>
    public class GameInfoResponse : Response
    {
        /// <summary>
        /// </summary>
        public GameInfoResponse()
            : base(Command.CmdGameOpen)
        {
        }

        /// <summary>
        /// </summary>
        public uint GameId;

        /// <summary>
        /// </summary>
        public string GameName;

        /// <summary>
        /// </summary>
        public string GameVersion;

        /// <summary>
        /// </summary>
        public string GameDll;

        /// <summary>
        /// </summary>
        public uint MaxLines;

        /// <summary>
        /// </summary>
        public uint MaxCredits;

        /// <summary>
        /// </summary>
        public uint Denomination;

        /// <summary>
        /// </summary>
        public uint PayoutPercentage;

        /// <summary>
        /// </summary>
        public uint[] ProgressiveIds;

        /// <summary>
        /// </summary>
        public uint[] ProgCreditsBet;

        /// <summary>
        /// </summary>
        public CRaceTicketSets RaceTicketSets;

        /// <summary>
        /// This contains the Prize location index for all RaceTicketSets which is used to
        ///  get ExtraWinnings for a pattern or Prize information in case of recovery
        /// </summary>
        public int[][] PrizeLocations;
    }
}