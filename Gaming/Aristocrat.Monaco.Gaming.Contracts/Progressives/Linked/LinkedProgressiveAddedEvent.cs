namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;

    /// <summary>
    ///     This event is posted when 1 or more linked progressive levels are added to the system.
    /// </summary>
    public class LinkedProgressiveAddedEvent : LinkedProgressiveEvent
    {
        /// <inheritdoc />
        public LinkedProgressiveAddedEvent(IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels)
            : base(linkedLevels)
        {
        }
    }
}