namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Central;
    using Contracts.Models;
    using Contracts.Progressives;

    /// <summary>
    ///     A game history log.
    /// </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Gaming.Contracts.IGameHistoryLog" />
    public class GameHistoryLog : IGameHistoryLog,IDisposable
    {
        private bool _disposed;
        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Gaming.GameHistoryLog class.
        /// </summary>
        /// <param name="index">Zero-based index of the memory block.</param>
        public GameHistoryLog(int index)
        {
            StorageIndex = index;
        }
        ~GameHistoryLog() => Dispose(false);
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
            }
            _disposed = true;
        }
        /// <summary>
        ///     Gets or sets the recovery BLOB.
        /// </summary>
        /// <value>The recovery BLOB.</value>
        public byte[] RecoveryBlob { get; set; }

        public GameConfiguration DenomConfiguration { get; set; }

        /// <inheritdoc/>
        public long TransactionId { get; set; }

        public long LogSequence { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public long EndTransactionId { get; set; }

        public int GameId { get; set; }

        public long DenomId { get; set; }

        public long StartCredits { get; set; }

        public long EndCredits { get; set; }

        public PlayState PlayState { get; set; }

        /// <summary>
        ///     Gets or sets the error code.
        /// </summary>
        /// <value>The error code.</value>
        public GameErrorCode ErrorCode { get; set; }

        public GameResult Result
        {
            get
            {
                switch (PlayState)
                {
                    case PlayState.FatalError:
                    case PlayState.Idle when FinalWager == 0:
                        return GameResult.Failed;
                    default:
                    {
                        if (PlayState != PlayState.GameEnded && PlayState != PlayState.Idle)
                        {
                            return GameResult.None;
                        }

                        return FinalWin > 0 ? GameResult.Won : GameResult.Lost;
                    }
                }
            }
        }

        public long InitialWager { get; set; }

        public long FinalWager { get; set; }

        public long PromoWager { get; set; }

        public long UncommittedWin { get; set; }

        public long InitialWin { get; set; }

        public long SecondaryPlayed { get; set; }

        public long SecondaryWager { get; set; }

        public long SecondaryWin { get; set; }

        public long FinalWin { get; set; }

        public long GameWinBonus { get; set; }

        public long TotalWon => FinalWin + GameWinBonus;

        public long AmountOut { get; set; }

        public DateTime LastUpdate { get; set; }

        public int LastCommitIndex { get; set; }

        public string GameRoundDescriptions { get; set; }

        public IEnumerable<Jackpot> JackpotSnapshot { get; set; }

        public IEnumerable<JackpotInfo> Jackpots { get; set; }

        public IEnumerable<TransactionInfo> Transactions { get; set; }

        public IEnumerable<GameEventLogEntry> Events { get; set; }

        public ICollection<GameRoundMeterSnapshot> MeterSnapshots { get; set; }

        public IEnumerable<FreeGame> FreeGames { get; set; }

        /// <summary>
        ///     Gets or sets the zero-based index of the free game.
        /// </summary>
        /// <value>The free game index.</value>
        public int FreeGameIndex { get; set; }

        public IEnumerable<CashOutInfo> CashOutInfo { get; set; }

        public IEnumerable<Outcome> Outcomes { get; set; }

        public int StorageIndex { get; set; }

        public IGameHistoryLog ShallowCopy()
        {
            return (IGameHistoryLog)MemberwiseClone();
        }

        public string LocaleCode { get; set; }

        public string GameConfiguration { get; set; }
    }
}