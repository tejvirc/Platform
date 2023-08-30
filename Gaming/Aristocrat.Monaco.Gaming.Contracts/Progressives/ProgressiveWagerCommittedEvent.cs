namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using Kernel;
    using System.Collections.Generic;

    /// <summary>
    ///     A <see cref="ProgressiveWagerCommittedEvent"/> is posted when a progressive wager has been committed
    /// </summary>
    public class ProgressiveWagerCommittedEvent : BaseEvent
    {
        /// <summary>
        ///     Gets the levels that were committed to
        /// </summary>
        public IReadOnlyCollection<ProgressiveLevel> Levels { get; }

        /// <summary>
        ///     The amount of the committed wager
        /// </summary>
        public long Wager { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveWagerCommittedEvent" /> class.
        /// </summary>
        /// <param name="levels">The affected levels</param>
        /// <param name="wager">The committed wager</param>
        public ProgressiveWagerCommittedEvent(IReadOnlyCollection<ProgressiveLevel> levels, long wager)
        {
            Levels = levels;
            Wager = wager;
        }


    }
}