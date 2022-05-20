namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;

    /// <summary>
    ///     This event is posted when a linked progressive is removed from the system.
    /// </summary>
    public class LinkedProgressiveRemovedEvent : LinkedProgressiveEvent
    {
        /// <summary>
        ///     Creates a new instance of the LinkedProgressiveRemoved event
        /// </summary>
        /// <param name="linkedLevels"></param>
        public LinkedProgressiveRemovedEvent(IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels)
            : base(linkedLevels)
        {
        }
    }
}