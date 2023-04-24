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
        public BingoGamePatternEvent(IReadOnlyCollection<BingoPattern> patterns, bool startPatternCycle = false, int gameIndex = 0)
        {
            Patterns = patterns;
            StartPatternCycle = startPatternCycle;
            GameIndex = gameIndex;
        }

        public IReadOnlyCollection<BingoPattern> Patterns { get; }

        public bool StartPatternCycle { get; }

        public int GameIndex { get; }
    }
}
