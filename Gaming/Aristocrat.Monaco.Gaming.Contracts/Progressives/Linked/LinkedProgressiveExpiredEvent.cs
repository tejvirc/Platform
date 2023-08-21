namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     This event is posted when 1 or more linked progressive levels has expired.
    /// </summary>
    public class LinkedProgressiveExpiredEvent : BaseEvent
    {
        /// <summary>
        ///     Creates a new instance of the ProgressiveBroadcastTimedOutEvent 
        /// </summary>
        /// <param name="newlyExpiredLevels">The levels that have timed out</param>
        /// <param name="preExistingExpiredLevels">Indicates if there were levels already expired.</param>
        public LinkedProgressiveExpiredEvent(
            IEnumerable<IViewableLinkedProgressiveLevel> newlyExpiredLevels,
            IEnumerable<IViewableLinkedProgressiveLevel> preExistingExpiredLevels)
        {
            NewlyExpiredLevels = newlyExpiredLevels;
            PreExistingExpiredLevels = preExistingExpiredLevels;
        }

        /// <summary>
        ///     Gets a collection of levels that are newly expired
        /// </summary>
        public IEnumerable<IViewableLinkedProgressiveLevel> NewlyExpiredLevels { get; }

        /// <summary>
        ///     Gets a collection of levels that were already expired
        /// </summary>
        public IEnumerable<IViewableLinkedProgressiveLevel> PreExistingExpiredLevels { get; }
    }
}