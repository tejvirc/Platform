namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.SharedSap
{
    using System.Collections.Generic;

    /// <summary>
    ///     Posted when a shared sap level is removed
    /// </summary>
    public class SharedSapRemovedEvent : SharedSapEvent
    {
        /// <summary>
        ///     Creates a new instance a of a SharedSapRemovedEvent
        /// </summary>
        /// <param name="sharedSapLevels">The levels that were removed</param>
        public SharedSapRemovedEvent(IEnumerable<IViewableSharedSapLevel> sharedSapLevels)
            : base(sharedSapLevels)
        {
        }
    }
}