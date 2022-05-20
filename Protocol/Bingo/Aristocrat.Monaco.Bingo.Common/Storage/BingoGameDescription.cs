namespace Aristocrat.Monaco.Bingo.Common.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameOverlay;
    using Gaming.Contracts.Central;

    [Serializable]
    public class BingoGameDescription : IOutcomeDescription
    {
        /// <summary>
        ///     The game serial number
        /// </summary>
        public long GameSerial { get; set; }

        /// <summary>
        ///     The numbers of the balls in the ball call.
        /// </summary>
        public IEnumerable<BingoNumber> BallCallNumbers { get; set; } = Enumerable.Empty<BingoNumber>();

        /// <summary>
        ///     The cards played for the round, including any winning patterns.
        /// </summary>
        public IEnumerable<BingoCard> Cards { get; set; } = Enumerable.Empty<BingoCard>();

        /// <summary>
        ///     The bingo patterns for the round
        /// </summary>
        public IEnumerable<BingoPattern> Patterns { get; set; } = Enumerable.Empty<BingoPattern>();

        /// <summary>
        ///     The progressive levels award for this game round
        /// </summary>
        public IEnumerable<long> ProgressiveLevels { get; set; } = Enumerable.Empty<long>();

        /// <summary>
        ///     The index of the first ball called when this game round was active.
        /// </summary>
        public int JoinBallIndex { get; set; }

        /// <summary>
        ///     Whether or not the Game End Win was claimed and approved for the round.
        /// </summary>
        public bool GameEndWinClaimAccepted { get; set; }

        /// <summary>
        ///     The game title id
        /// </summary>
        public uint GameTitleId { get; set; }

        /// <summary>
        ///     The game theme id
        /// </summary>
        public uint ThemeId { get; set; }

        /// <summary>
        ///     The game denomination id
        /// </summary>
        public int DenominationId { get; set; }

        /// <summary>
        ///     The paytable used for the base game outcome
        /// </summary>
        public string Paytable { get; set; }

        /// <summary>
        ///     The facade key
        /// </summary>
        public int FacadeKey { get; set; }

        /// <summary>
        ///     The date time for when the game joined
        /// </summary>
        public DateTime JoinTime { get; set; }

        /// <summary>
        ///     The game end win eligibility bit
        /// </summary>
        public int GameEndWinEligibility { get; set; }
    }
}
