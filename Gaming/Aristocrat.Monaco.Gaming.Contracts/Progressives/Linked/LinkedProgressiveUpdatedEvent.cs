namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;

    /// <summary>
    ///     Posted whenever a linked progressive level is updated.
    /// </summary>
    public class LinkedProgressiveUpdatedEvent : LinkedProgressiveEvent
    {
        /// <summary>
        ///     Creates a new instance of the LinkedProgressiveAdded event
        /// </summary>
        /// <param name="linkedLevels"></param>
        public LinkedProgressiveUpdatedEvent(IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels) :
            base (linkedLevels)
        {
        }
    }
}