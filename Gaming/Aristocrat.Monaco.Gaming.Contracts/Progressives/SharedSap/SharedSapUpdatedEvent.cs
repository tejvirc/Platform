namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.SharedSap
{
    using System.Collections.Generic;

    /// <summary>
    ///     Posted when 1 or more shared sap level is updated
    /// </summary>
    public class SharedSapUpdatedEvent : SharedSapEvent
    {
        /// <summary>
        ///     Creates a new instance of the SharedSapUpdatedEvent
        /// </summary>
        /// <param name="sharedSapLevels">The levels that were updated</param>
        public SharedSapUpdatedEvent(IEnumerable<IViewableSharedSapLevel> sharedSapLevels)
            : base(sharedSapLevels)
        {
        }
    }
}