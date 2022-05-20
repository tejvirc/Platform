namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.SharedSap
{
    using System.Collections.Generic;

    /// <summary>
    ///     Posted when 1 or more shared sap levels have been added
    /// </summary>
    public class SharedSapAddedEvent : SharedSapEvent
    {
        /// <summary>
        ///     Creates a new instance of a SharedSapAdded event
        /// </summary>
        /// <param name="sharedSapLevels">The levels that were added</param>
        public SharedSapAddedEvent(IEnumerable<IViewableSharedSapLevel> sharedSapLevels)
            : base(sharedSapLevels)
        {
        }
    }
}