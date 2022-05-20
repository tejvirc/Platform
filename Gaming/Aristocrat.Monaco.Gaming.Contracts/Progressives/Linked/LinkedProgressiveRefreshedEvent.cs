namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;

    /// <summary>
    ///     This event is posted when 1 or more expired linked progressive levels are updated
    /// </summary>
    public class LinkedProgressiveRefreshedEvent : LinkedProgressiveEvent
    {
        /// <summary>
        ///     Creates a new instance of the LinkedProgressiveRefreshedEvent 
        /// </summary>
        /// <param name="levels">The levels that have timed out</param>
        public LinkedProgressiveRefreshedEvent(IEnumerable<IViewableLinkedProgressiveLevel> levels)
            : base(levels)
        {
        }
    }
}