namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Collections.Generic;
    using Client.Data;
    using Gaming.Contracts.Central;

    /// <summary>
    ///     Stores the various amounts that make up a win for a HHR game round, which
    /// </summary>
    public class PrizeInformation
    {
        /// <summary>
        ///     The amount wagered on race set 1.
        /// </summary>
        public uint RaceSet1Wager;

        /// <summary>
        ///     The amount wagered on race set 2.
        /// </summary>
        public uint RaceSet2Wager;

        /// <summary>
        ///     The amount won from matching patterns on race set 1.
        /// </summary>
        public long RaceSet1AmountWon;

        /// <summary>
        ///     The amount won from matching patterns on race set 2, including progressive won amount.
        /// </summary>
        public long RaceSet2AmountWon => RaceSet2AmountWonWithoutProgressives + TotalProgressiveAmountWon;

        /// <summary>
        ///     The amount won from race set 2 excluding any progressive won amount.
        /// </summary>
        public long RaceSet2AmountWonWithoutProgressives;
        
        /// <summary>
        ///     Extra winnings for race set 1
        /// </summary>
        public uint RaceSet1ExtraWinnings;

        /// <summary>
        ///     Extra winnings for race set 2
        /// </summary>
        public uint RaceSet2ExtraWinnings;

        /// <summary>
        /// </summary>
        public ulong ScratchTicketSetId { get; set; }

        /// <summary>
        /// </summary>
        public ulong ScratchTicketId { get; set; }

        /// <summary>
        /// </summary>
        public uint ReplyId { get; set; }

        /// <summary>
        ///     The amount won from progressive prizes on race set 2.
        /// </summary>
        public long TotalProgressiveAmountWon;

        /// <summary>
        ///     A collection showing which progressive level was hit and how many times.
        /// </summary>
        public IReadOnlyCollection<(int levelId, int count)> ProgressiveLevelsHit;

        /// <summary>
        ///     Information associated with RaceInfo.iProgWon[] indicating amount of progressive won for each progressive level.
        /// </summary>
        public IReadOnlyCollection<uint> ProgressiveWin;

        /// <summary>
        ///     Indicates whether game is overridden
        /// </summary>
        public bool BOverride;

        /// <summary>
        ///     Last Game played time as received from the server. This will be used in Transaction messages sent by EGM.
        /// </summary>
        public uint LastGamePlayedTime;

        /// <summary>
        ///     The RaceInfo response received from the server
        /// </summary>
        public CRaceInfo RaceInfo;

        // ReSharper disable once UnusedMember.Global
        public IReadOnlyCollection<(int levelId, long amount)> ProgressiveLevelAmountHit { get; set; }

        /// <summary>
        ///     The transaction ID that was associated with the game play that just occurred.
        /// </summary>
        public long TransactionId;

        /// <summary>
        ///     The game ID that was associated with the game play that just occurred.
        /// </summary>
        public uint GameMapId;

        /// <summary>
        ///     If we do a handpay for this prize, we'll need to have this Guid persisted so we can find the corresponding prize
        ///     after the handpay is cleared.
        /// </summary>
        public Guid RaceSet1HandpayGuid;

        /// <summary>
        ///     If we're in LargeWinCashOutStrategy.Handpay mode and the handpay is keyed off with a voucher, we'll get another
        ///     event for the same handpay, so we have to record that a handpay has been keyed off.
        /// </summary>
        public bool RaceSet1HandpayKeyedOff;

        /// <summary>
        ///     If we do a handpay for this prize, we'll need to have this Guid persisted so we can find the corresponding prize
        ///     after the handpay is cleared.
        /// </summary>
        public Guid RaceSet2HandpayGuid;

        /// <summary>
        ///     If we're in LargeWinCashOutStrategy.Handpay mode and the handpay is keyed off with a voucher, we'll get another
        ///     event for the same handpay, so we have to record that a handpay has been keyed off.
        /// </summary>
        public bool RaceSet2HandpayKeyedOff;

        /// <summary>
        ///     List of Outcomes extracted from the incoming prize information from server.
        /// </summary>
        public IEnumerable<Outcome> Outcomes;

        /// <summary>
        ///     String representation of the race information received from server as part of game play response.
        /// </summary>
        public string GameRoundInfo;

        /// <summary>
        ///     The denomination of the game in cents.
        /// </summary>
        public uint Denomination;
    }
}