namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;

    /// <summary>
    ///     This event is posted when a linked progressive host reports that when a link is up
    /// </summary>
    public class LinkedProgressiveConnectedEvent : LinkedProgressiveEvent
    {
        /// <summary>
        ///     Creates a new instance of the LinkedProgressiveConnectedEvent class
        /// </summary>
        /// <param name="levels">The levels associated with the connected event</param>
        public LinkedProgressiveConnectedEvent(IEnumerable<IViewableLinkedProgressiveLevel> levels)
            : base(levels)
        {
        }
    }
}