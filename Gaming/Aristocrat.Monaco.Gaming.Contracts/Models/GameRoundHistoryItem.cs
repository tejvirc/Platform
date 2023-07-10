namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    /// <summary>
    ///     Data for each game history entry.
    /// </summary>
    public class GameRoundHistoryItem
    {
        /// <summary> Gets or sets game reference number as text.</summary>
        public string RefNoText { get; set; }

        /// <summary> Gets or sets game Id.</summary>
        public int GameId { get; set; }

        /// <summary> Gets or sets game name.</summary>
        public string GameName { get; set; }

        /// <summary> Gets or sets game version.</summary>
        public string GameVersion { get; set; }

        /// <summary> Gets or sets the denom in millicents.</summary>
        public long DenomId { get; set; }

        /// <summary> Gets or sets the denom in dollars.</summary>
        public decimal Denom { get; set; }

        /// <summary> Gets or sets the start time.</summary>
        public DateTime StartTime { get; set; }

        /// <summary> Gets or sets the ending time.</summary>
        public DateTime EndTime { get; set; }

        /// <summary> Gets or sets the start credits in dollars.</summary>
        public decimal StartCredits { get; set; }

        /// <summary> Gets or sets the credits wagered.</summary>
        public decimal? CreditsWagered { get; set; }

        /// <summary> Gets or sets the credits won.</summary>
        public decimal? CreditsWon { get; set; }

        /// <summary> Gets or sets the credits in prior to the game round beginning.</summary>
        public decimal? AmountIn { get; set; }

        /// <summary> Gets or sets the credits out after the game round.</summary>
        public decimal? AmountOut { get; set; }

        /// <summary> Gets or sets the end credits in dollars.</summary>
        public decimal? EndCredits { get; set; }

        /// <summary> Gets or sets the end jackpot amounts as a formatted string.</summary>
        public string EndJackpot { get; set; }

        /// <summary> Gets or sets the end jackpot values.</summary>
        public IEnumerable<Jackpot> EndJackpots { get; set; }

        /// <summary> Gets or sets the last play state recorded.</summary>
        public PlayState GameState { get; set; }

        /// <summary> Gets or sets the replay index of this item.</summary>
        public int ReplayIndex { get; set; }

        /// <summary> Gets or sets the free game index of this item.</summary>
        public int GameIndex { get; set; } = -1;

        /// <summary> Gets or sets the replay index of this item.</summary>
        public long LogSequence { get; set; }

        /// <summary> Gets the LogSequence Text</summary>
        public string LogSequenceText => !IsTransactionItem ? LogSequence.ToString() : string.Empty;

        /// <summary> Gets or sets the game round description text.</summary>
        public string GameRoundDescriptionText { get; set; }

        /// <summary> Gets or sets a value indicating whether the game can be replayed.</summary>
        public bool CanReplay { get; set; }

        /// <summary> Gets or sets a value indicating the status.</summary>
        public string Status { get; set; }

        /// <summary> Gets the DateTime in UTC</summary>
        public DateTime UtcStartTime => StartTime.ToUniversalTime();

        /// <summary> TransactionId for transaction entries </summary>
        public long? TransactionId { get; set; }

        /// <summary> Determines if the item is a Transaction (vs Game History) Item </summary>
        public bool IsTransactionItem => TransactionId.HasValue;
    }
}
