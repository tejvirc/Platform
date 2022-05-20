namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;

    /// <summary>
    ///     An event to handling the claim timeout being cleared
    /// </summary>
    public class LinkedProgressiveClaimRefreshedEvent : LinkedProgressiveEvent
    {
        /// <summary>
        ///     Creates the claim timeout cleared event
        /// </summary>
        /// <param name="linkedLevels">The levels that had the claim timeout cleared</param>
        public LinkedProgressiveClaimRefreshedEvent(IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels)
            : base(linkedLevels)
        {
        }
    }
}