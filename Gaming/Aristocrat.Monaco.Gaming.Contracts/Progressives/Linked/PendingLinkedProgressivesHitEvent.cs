namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    ///     This event is posted when 1 or more pending linked progressive levels are hit.
    /// </summary>
    public class PendingLinkedProgressivesHitEvent : LinkedProgressiveEvent
    {
        /// <inheritdoc />
        public PendingLinkedProgressivesHitEvent(IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels)
            : base(linkedLevels)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var levels = new StringBuilder();
            foreach (var level in LinkedProgressiveLevels)
            {
                levels.Append($"level: {level.LevelName}");
            }

            return $"Pending Linked Jackpots {levels}";
        }
    }
}