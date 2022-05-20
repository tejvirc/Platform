namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;

    /// <summary>
    ///     This event is posted when a linked progressive host reports a link down
    /// </summary>
    public class LinkedProgressiveDisconnectedEvent : LinkedProgressiveEvent
    {
        /// <summary>
        ///     Creates a new instance of the ProgressiveLinkDownEvent
        /// </summary>
        /// <param name="linkedLevels">The levels that are link down</param>
        public LinkedProgressiveDisconnectedEvent(IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels) :
            base(linkedLevels)
        {
        }
    }
}