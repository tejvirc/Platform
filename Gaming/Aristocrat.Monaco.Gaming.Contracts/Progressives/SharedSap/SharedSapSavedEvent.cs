namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.SharedSap
{
    using System.Collections.Generic;

    /// <summary>
    ///     Posted when 1 or more shared sap levels have been saved to persistence
    /// </summary>
    public class SharedSapSavedEvent : SharedSapEvent
    {
        /// <summary>
        ///     Creates a new instance of the SharedSapSavedEvent
        /// </summary>
        /// <param name="sharedSapLevels">The levels that were saved</param>
        public SharedSapSavedEvent(IEnumerable<IViewableSharedSapLevel> sharedSapLevels)
            : base(sharedSapLevels)
        {
        }
    }
}