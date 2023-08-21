namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     Abstract base class for linked progressive events.
    /// </summary>
    public abstract class LinkedProgressiveEvent : BaseEvent
    {
        /// <summary>
        ///     Creates a new instance of the LinkedProgressiveEvent event
        /// </summary>
        /// <param name="linkedLevels">The levels associated with the event</param>
        protected LinkedProgressiveEvent(IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels)
        {
            LinkedProgressiveLevels = linkedLevels;
        }

        /// <summary>
        ///     Gets the linked progressive levels associated with the event
        /// </summary>
        public IEnumerable<IViewableLinkedProgressiveLevel> LinkedProgressiveLevels { get; }
    }
}