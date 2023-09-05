namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System;
    using System.Collections.Generic;
    using ServerApiGateway;

    /// <summary>
    ///     Contains the information for a single game outcome
    /// </summary>
    public class SingleGameOutcomeMessage : IMessage
    {
        /// <summary>The total amount won </summary>
        public long TotalWinAmount { get; set; }

        /// <summary>The facade key used to look up a presentation </summary>
        public string FacadeKey { get; set; }

        /// <summary>The game title id </summary>
        public uint GameTitleId { get; set; }

        /// <summary>The denomination used during the game </summary>
        public int Denomination { get; set; }

        /// <summary>The balance at the start of the game </summary>
        public long InitialBalance { get; set; }

        /// <summary>The amount paid back to the player </summary>
        public long PaidAmount { get; set; }

        /// <summary>The amount bet for this game </summary>
        public long BetAmount { get; set; }

        /// <summary>The final balance at the end of the game </summary>
        public long FinalBalance { get; set; }

        /// <summary>The time the game was started </summary>
        public DateTime StartTime { get; set; }

        /// <summary>The time the game was joined </summary>
        public DateTime JoinTime { get; set; }

        /// <summary>The presentation number used to show the win </summary>
        public long PresentationNumber { get; set; }

        /// <summary>The theme id of the game </summary>
        public uint ThemeId { get; set; }

        /// <summary>The game number this outcome is associated with </summary>
        public int GameNumber { get; set; }

        /// <summary>The pay table id for the game </summary>
        public int PaytableId { get; set; }

        /// <summary>The list of cards played </summary>
        public IEnumerable<BingoSingleGameOutcomeMeta.Types.CardPlayed> Cards { get; set; }

        /// <summary>The list of wins </summary>
        public IEnumerable<BingoSingleGameOutcomeMeta.Types.WinResult> Wins { get; set; }
    }
}