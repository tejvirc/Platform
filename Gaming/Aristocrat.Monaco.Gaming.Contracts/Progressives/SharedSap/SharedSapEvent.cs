namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.SharedSap
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     Abstract base class for shared standalone progressive events
    /// </summary>
    public abstract class SharedSapEvent : BaseEvent
    {
        /// <summary>
        ///     Creates a new instance of the SharedSapEvent event
        /// </summary>
        /// <param name="sharedSapLevels">The shared sap levels related to the events</param>
        protected SharedSapEvent(IEnumerable<IViewableSharedSapLevel> sharedSapLevels)
        {
            SharedSapLevels = sharedSapLevels;
        }

        /// <summary>
        ///     Gets the shared sap levels associated with the event
        /// </summary>
        public IEnumerable<IViewableSharedSapLevel> SharedSapLevels { get; }
    }
}