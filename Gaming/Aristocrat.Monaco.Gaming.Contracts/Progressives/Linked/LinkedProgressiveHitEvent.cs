namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;

    /// <summary>
    ///     This event is posted when 1 or more linked progressive levels are hit.
    /// </summary>
    public class LinkedProgressiveHitEvent : LinkedProgressiveEvent
    {
        /// <summary>
        ///     Sets the level.
        /// </summary>
        public IViewableProgressiveLevel Level { get; }

        /// <summary>
        ///     Gets the remaining amount that can be claimed
        /// </summary>
        public double? RemainingAmount { get; }

        /// <inheritdoc />
        public LinkedProgressiveHitEvent(IViewableProgressiveLevel level, IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels, double? remainingAmount = default)
            : base(linkedLevels)
        {
            Level = level;
            RemainingAmount = remainingAmount;
        }
    }
}