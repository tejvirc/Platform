namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using Data;

    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_GAME_RECOVER_RESPONSE
    ///     Struct:   GMessageGameRecoverResponse
    ///     Request:  GMessageGameRecover
    /// </summary>
    public class GameRecoveryResponse : Response
    {
        /// <summary>
        /// </summary>
        public GameRecoveryResponse()
            : base(Command.CmdGameRecoverResponse)
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
        public uint RaceTicketSetId;

        /// <summary>
        /// </summary>
        public uint RaceTicketId;

        /// <summary>
        /// </summary>
        public uint PrizeLoc1;

        /// <summary>
        /// </summary>
        public uint PrizeLoc2;

        /// <summary>
        /// </summary>
        public uint LastGamePlayTime;

        /// <summary>
        /// </summary>
        public CRaceInfo RaceInfo;
    }
}