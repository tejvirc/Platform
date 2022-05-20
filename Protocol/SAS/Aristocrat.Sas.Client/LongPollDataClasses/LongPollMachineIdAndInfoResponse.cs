namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Response class to hold the response of LP 1F - Send Machine ID And Info
    /// </summary>
    public class LongPollMachineIdAndInfoResponse : LongPollResponse
    {
        /// <summary>
        ///     Returns new empty LongPollMachineIdAndInfoResponse.
        /// </summary>
        public LongPollMachineIdAndInfoResponse ()
        {
        }

        /// <summary>
        ///     Returns new LongPollMachineIdAndInfoResponse with fields filled in.
        /// </summary>
        /// <param name="gameId">Game ID.</param>
        /// <param name="addId">Additional ID.</param>
        /// <param name="denomination">Denomination.</param>
        /// <param name="maxBet">Max Bet.</param>
        /// <param name="progGroup">Progressive Group.</param>
        /// <param name="gameOptions">Game Options.</param>
        /// <param name="paytableId">Paytable ID.</param>
        /// <param name="basePercent">Theoretical RTP Percent.</param>
        public LongPollMachineIdAndInfoResponse (
            string gameId, string addId, byte denomination, byte maxBet, byte progGroup, uint gameOptions, string paytableId, string basePercent
        )
        {
            GameId = gameId;
            AdditionalId = addId;
            Denomination = denomination;
            MaxBet = maxBet;
            ProgressiveGroup = progGroup;
            GameOptions = gameOptions;
            PaytableId = paytableId;
            TheoreticalRtpPercent = basePercent;
        }

        /// <summary>
        ///     IGT-assigned 2-byte string representing game publisher.
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        ///     Space for extra game ID info. If game doesn't support additional ID this should return "000"
        /// </summary>
        public string AdditionalId { get; set; }

        /// <summary>
        ///     SAS accounting denomination.
        /// </summary>
        public byte Denomination { get; set; }

        /// <summary>
        ///     Largest configured Max Bet for machine, or FF if too large for 1 byte
        /// </summary>
        public byte MaxBet { get; set; }

        /// <summary>
        ///     Curently configured progressive group ID.
        /// </summary>
        public byte ProgressiveGroup { get; set; }

        /// <summary>
        ///     Optional 2-byte space for extra info.
        /// </summary>
        public uint GameOptions { get; set; }

        /// <summary>
        ///     String representing paytable ID. All game combos should have unique or semi-unique IDs.
        /// </summary>
        public string PaytableId { get; set; }

        /// <summary>
        ///     4-byte string representing max RTP (averaged across multiple games). Decimal of percentage is implied and not transmitted (e.g. "9091").
        /// </summary>
        public string TheoreticalRtpPercent { get; set; }
    }
}
