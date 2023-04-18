namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Holds information about a multi-game outcome
    /// </summary>
    public class ReportMultiGameOutcomeMessage : IMessage
    {
        /// <summary>Gets or sets the Transaction Id</summary>
        public long TransactionId { get; set; }

        /// <summary>Gets or sets the machine serial number</summary>
        public string MachineSerial { get; set; } = string.Empty;

        /// <summary>Gets or sets the bet amount</summary>
        public long BetAmount { get; set; }

        /// <summary>Gets or sets the total win</summary>
        public long TotalWin { get; set; }

        /// <summary>Gets or sets the paid amount</summary>
        public long PaidAmount { get; set; }

        /// <summary>Gets or sets the TransactionId</summary>
        public long StartingBalance { get; set; }

        /// <summary>Gets or sets the final balance</summary>
        public long FinalBalance { get; set; }

        /// <summary>Gets or sets the facade key</summary>
        public int FacadeKey { get; set; }

        /// <summary>Gets or sets the TransactionId</summary>
        public long PresentationIndex { get; set; }

        /// <summary>Gets or sets the game end win eligibility</summary>
        public int GameEndWinEligibility { get; set; }

        /// <summary>Gets or sets the game title Id</summary>
        public uint GameTitleId { get; set; }

        /// <summary>Gets or sets the theme Id</summary>
        public uint ThemeId { get; set; }

        /// <summary>Gets or sets the denomination Id</summary>
        public int DenominationId { get; set; }

        /// <summary>Gets or sets the game serial number</summary>
        public long GameSerial { get; set; }

        /// <summary>Gets or sets the paytable name</summary>
        public string Paytable { get; set; } = string.Empty;

        /// <summary>Gets or sets the join ball</summary>
        public int JoinBall { get; set; }

        /// <summary>Gets or sets the start time</summary>
        public DateTime StartTime { get; set; }

        /// <summary>Gets or sets the join time</summary>
        public DateTime JoinTime { get; set; }

        /// <summary>Gets or sets the progressive levels</summary>
        public IEnumerable<long> ProgressiveLevels { get; set; } = Enumerable.Empty<long>();

        /// <summary>Gets or sets the bingo cards played</summary>
        public IEnumerable<CardPlayed> CardsPlayed { get; set; } = Enumerable.Empty<CardPlayed>();

        /// <summary>Gets or sets the ball call numbers</summary>
        public IEnumerable<int> BallCall { get; set; } = Enumerable.Empty<int>();

        /// <summary>Gets or sets the Win results</summary>
        public IEnumerable<WinResult> WinResults { get; set; } = Enumerable.Empty<WinResult>();
    }
}