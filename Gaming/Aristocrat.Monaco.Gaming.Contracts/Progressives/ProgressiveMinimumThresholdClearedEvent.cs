namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     An event for any progressive levels that has cleared the minimum threshold error for the game
    /// </summary>
    public class ProgressiveMinimumThresholdClearedEvent : BaseEvent
    {
        /// <summary>
        ///     Creates the minimum threshold error cleared event
        /// </summary>
        /// <param name="levels">The levels whose minimum threshold error was cleared</param>
        public ProgressiveMinimumThresholdClearedEvent(IEnumerable<IViewableProgressiveLevel> levels)
        {
            ProgressiveLevels = levels;
        }

        /// <summary>
        ///     Gets the linked progressive levels associated with the event
        /// </summary>
        public IEnumerable<IViewableProgressiveLevel> ProgressiveLevels { get; }
    }
}