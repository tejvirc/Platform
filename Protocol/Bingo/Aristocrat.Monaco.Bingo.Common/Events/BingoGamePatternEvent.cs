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
        public BingoGamePatternEvent(IReadOnlyCollection<BingoPattern> patterns, bool startPatternCycle = false)
        {
            Patterns = patterns;
            StartPatternCycle = startPatternCycle;
        }

        public IReadOnlyCollection<BingoPattern> Patterns { get; }

        public bool StartPatternCycle { get; }
    }
}
