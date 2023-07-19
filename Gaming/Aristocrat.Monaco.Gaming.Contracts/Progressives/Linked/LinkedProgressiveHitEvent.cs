namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     This event is posted when 1 or more linked progressive levels are hit.
    /// </summary>
    public class LinkedProgressiveHitEvent : LinkedProgressiveEvent
    {
        /// <summary>
        ///     Gets the level.
        /// </summary>
        public IViewableProgressiveLevel Level { get; }

        /// <summary>
        ///     Gets the associated transaction.
        /// </summary>
        public JackpotTransaction Jackpot { get; }

        /// <inheritdoc />
        public LinkedProgressiveHitEvent(IViewableProgressiveLevel level, IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels, ICloneable jackpot)
            : base(linkedLevels)
        {
            Level = level;
            Jackpot = (JackpotTransaction)jackpot.Clone();
        }
    }
}