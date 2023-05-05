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
        ///     Gets the associated transaction id.
        /// </summary>
        public long TransactionId { get; }

        /// <inheritdoc />
        public LinkedProgressiveHitEvent(IViewableProgressiveLevel level, IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels, long transactionId)
            : base(linkedLevels)
        {
            Level = level;
            TransactionId = transactionId;
        }
    }
}