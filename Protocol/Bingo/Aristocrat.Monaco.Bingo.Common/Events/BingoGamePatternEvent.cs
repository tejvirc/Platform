namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using System.Collections.Generic;
    using GameOverlay;
    using Kernel;

    /// <summary>
    ///     Bingo Game pattern event.
    /// </summary>
    public class BingoGamePatternEvent : BaseEvent
    {
        public BingoGamePatternEvent(
            IReadOnlyCollection<BingoPattern> patterns,
            bool startPatternCycle = false,
            int gameIndex = 0)
        {
            Patterns = patterns;
            StartPatternCycle = startPatternCycle;
            GameIndex = gameIndex;
        }

        /// <summary>Gets the patterns on the card</summary>
        public IReadOnlyCollection<BingoPattern> Patterns { get; }

        /// <summary>Gets whether the pattern cycling should start</summary>
        public bool StartPatternCycle { get; }

        /// <summary>Gets the game that these patterns belong to</summary>
        public int GameIndex { get; }
    }
}
