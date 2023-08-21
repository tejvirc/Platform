namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     An event for any progressive levels that do not meet the minimum threshold for the game
    /// </summary>
    public class ProgressiveMinimumThresholdErrorEvent : BaseEvent
    {
        /// <summary>
        ///     Creates the minimum threshold error event
        /// </summary>
        /// <param name="levels">The levels that have the minimum threshold error</param>
        public ProgressiveMinimumThresholdErrorEvent(IEnumerable<IViewableProgressiveLevel> levels)
        {
            ProgressiveLevels = levels;
        }

        /// <summary>
        ///     Gets the progressive levels associated with the event
        /// </summary>
        public IEnumerable<IViewableProgressiveLevel> ProgressiveLevels { get; }
    }
}