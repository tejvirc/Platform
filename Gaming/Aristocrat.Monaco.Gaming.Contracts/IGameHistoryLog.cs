namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Central;
    using Models;
    using Progressives;

    /// <summary>
    ///     Represents a game history record
    /// </summary>
    public interface IGameHistoryLog : ILogSequence
    {
        /// <summary>
        ///     Gets the globally unique transaction identifier
        /// </summary>
        long TransactionId { get; }

        /// <summary>
        ///     Gets the start date time
        /// </summary>
        DateTime StartDateTime { get; }

        /// <summary>
        ///     Gets the start date time
        /// </summary>
        DateTime EndDateTime { get; }

        /// <summary>
        ///     Gets the globally unique transaction identifier for the end of the game
        /// </summary>
        long EndTransactionId { get; }

        /// <summary>
        ///     Gets the game identifier
        /// </summary>
        int GameId { get; }

        /// <summary>
        ///     Gets the denomination identifier
        /// </summary>
        long DenomId { get; }

        /// <summary>
        ///     Gets the starting credits
        /// </summary>
        long StartCredits { get; }

        /// <summary>
        ///     Gets the ending credits
        /// </summary>
        long EndCredits { get; }

        /// <summary>
        ///     Gets the current state of game play
        /// </summary>
        PlayState PlayState { get; }

        /// <summary>
        ///     Gets the result of the game
        /// </summary>
        GameResult Result { get; }

        /// <summary>
        ///     Gets the initial amount wagered
        /// </summary>
        long InitialWager { get; }

        /// <summary>
        ///     Gets the final amount wagered in the primary game
        /// </summary>
        long FinalWager { get; }

        /// <summary>
        ///     Get the amount wagered from the promo player bank
        /// </summary>
        long PromoWager { get; }

        /// <summary>
        ///     Gets the Uncommitted win from primary game
        /// </summary>
        /// <remarks>
        ///     This value is not stored and is only valid until the Primary Game Ends
        /// </remarks>
        long UncommittedWin { get; }

        /// <summary>
        ///     Gets the Initial win from primary game, including progressive win amounts
        /// </summary>
        long InitialWin { get; }

        /// <summary>
        ///     Gets the count of secondary games played during all secondary game cycles played
        /// </summary>
        long SecondaryPlayed { get; }

        /// <summary>
        ///     Gets the Total of the initial amounts wagered on all secondary game cycles played
        /// </summary>
        long SecondaryWager { get; }

        /// <summary>
        ///     Gets the total of the final amounts won on all secondary game cycles played
        /// </summary>
        long SecondaryWin { get; }

        /// <summary>
        ///     Gets the final amount won
        /// </summary>
        long FinalWin { get; }

        /// <summary>
        ///     Gets the amount out attributed to the game round
        /// </summary>
        long AmountOut { get; }

        /// <summary>
        ///     Gets the amount awarded as paytable wins after the presentation completed
        /// </summary>
        long GameWinBonus { get; }

        /// <summary>
        ///     Gets the total amount awarded from the entire game round
        /// </summary>
        long TotalWon { get; }

        /// <summary>
        ///     Gets the date time the log record was last updated
        /// </summary>
        DateTime LastUpdate { get; }

        /// <summary>
        ///     Gets the index of the last committed game round (-1 for none, 0 for the base game, and 1+ for each free game)
        /// </summary>
        int LastCommitIndex { get; }

        /// <summary>
        ///     Gets the game round descriptions up to the last logged game round
        /// </summary>
        string GameRoundDescriptions { get; }

        /// <summary>
        ///     Gets the locale code (e.g., "fr-ca") for the game round
        /// </summary>
        string LocaleCode { get; }

        /// <summary>
        ///     Gets the Game Configuration for the game round
        /// </summary>
        string GameConfiguration { get; }

        /// <summary>
        ///     Gets the jackpot associated to the game round
        /// </summary>
        IEnumerable<JackpotInfo> Jackpots { get; }

        /// <summary>
        ///     Gets the transactions associated to the game round
        /// </summary>
        IEnumerable<TransactionInfo> Transactions { get; }

        /// <summary>
        ///     Gets the events associated to the game round
        /// </summary>
        IEnumerable<GameEventLogEntry> Events { get; }

        /// <summary>
        ///     Gets the meter snapshots associated with the game round.
        /// </summary>
        ICollection<GameRoundMeterSnapshot> MeterSnapshots { get; }

        /// <summary>
        ///     Gets the free associated to the game round
        /// </summary>
        IEnumerable<FreeGame> FreeGames { get; }

        /// <summary>
        ///     Gets the associated cashout info for the game round
        /// </summary>
        IEnumerable<CashOutInfo> CashOutInfo { get; }

        /// <summary>
        ///     Gets the associated outcomes for the game round.  This is only applicable for a central determinant game.
        /// </summary>
        IEnumerable<Outcome> Outcomes { get; }

        /// <summary>
        ///     Gets the storage index within the log
        /// </summary>
        int StorageIndex { get; set; }

        /// <summary>
        ///     Gets the initial jackpot data for the game round
        /// </summary>
        IEnumerable<Jackpot> JackpotSnapshot { get; }

        /// <summary>
        ///     Gets the final jackpot data after the game round has ended
        /// </summary>
        IEnumerable<Jackpot> JackpotSnapshotEnd { get; }

        /// <summary>
        ///     Gets the recovery blob
        /// </summary>
        byte[] RecoveryBlob { get; }

        /// <summary>
        ///     The configuration information for that game round
        /// </summary>
        GameConfiguration DenomConfiguration { get; }

        /// <summary>
        ///     Creates a shallow copy of the log
        /// </summary>
        /// <returns></returns>
        IGameHistoryLog ShallowCopy();

        /// <summary>
        ///     Gets the game round details
        /// </summary>
        GameRoundDetails GameRoundDetails { get; }
    }

    /// <summary>
    ///     (Serializable) game configuration
    /// </summary>
    [Serializable]
    public class GameConfiguration : IGameConfiguration
    {
        /// <inheritdoc />
        public int MinimumWagerCredits { get; set; }

        /// <inheritdoc />
        public int MaximumWagerCredits { get; set; }

        /// <inheritdoc />
        public int MaximumWagerOutsideCredits { get; set; }

        /// <inheritdoc />
        public string BetOption { get; set; }

        /// <inheritdoc />
        public string LineOption { get; set; }

        /// <inheritdoc />
        public int BonusBet { get; set; }

        /// <inheritdoc />
        public bool SecondaryEnabled { get; set; }

        /// <inheritdoc />
        public bool LetItRideEnabled { get; set; }
    }

    /// <summary>
    ///     (Serializable) a free game.
    /// </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Gaming.Contracts.IFreeGameInfo" />
    [Serializable]
    public class FreeGame : IFreeGameInfo
    {
        /// <inheritdoc />
        public DateTime StartDateTime { get; set; }

        /// <inheritdoc />
        public DateTime EndDateTime { get; set; }

        /// <inheritdoc />
        public long StartCredits { get; set; }

        /// <inheritdoc />
        public long EndCredits { get; set; }

        /// <inheritdoc />
        public long FinalWin { get; set; }

        /// <inheritdoc />
        public GameResult Result
        {
            get
            {
                if (EndDateTime == DateTime.MinValue)
                {
                    return GameResult.None;
                }

                return FinalWin > 0 ? GameResult.Won : GameResult.Lost;
            }
        }

        /// <inheritdoc />
        public long AmountOut { get; set; }
    }
}
