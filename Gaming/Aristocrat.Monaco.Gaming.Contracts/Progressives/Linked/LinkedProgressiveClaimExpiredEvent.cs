namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System.Collections.Generic;

    /// <summary>
    ///     An event for handling the claim timeout has expired
    /// </summary>
    public class LinkedProgressiveClaimExpiredEvent : LinkedProgressiveEvent
    {
        /// <summary>
        ///     Creates the claim timeout event
        /// </summary>
        /// <param name="linkedLevels">The claim levels that timed out</param>
        public LinkedProgressiveClaimExpiredEvent(IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels)
            : base(linkedLevels)
        {
        }
    }
}