namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using Data;

    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_GAME_BONANZA
    ///     Struct:   SMessageGameBonanza
    ///     Request:  GMessageGamePlay, GMessageRaceStart
    /// </summary>
    public class GamePlayResponse : Response
    {
        /// <summary>
        /// </summary>
        public GamePlayResponse()
            : base(Command.CmdGameBonanza)
        {
        }

        /// <summary>
        /// </summary>
        public uint GameId;

        /// <summary>
        /// </summary>
        public uint GameNo;

        /// <summary>
        /// </summary>
        public uint SeqNo;

        /// <summary>
        /// </summary>
        public bool BOverride;

        /// <summary>
        /// </summary>
        public uint ScratchTicketSetId;

        /// <summary>
        /// </summary>
        public uint ScratchTicketId;

        /// <summary>
        /// </summary>
        public string Prize;

        /// <summary>
        /// </summary>
        public CRaceInfo RaceInfo;

        /// <summary>
        /// </summary>
        public ulong HandicapEnter;

        /// <summary>
        /// </summary>
        public uint LastGamePlayTime;
    }
}