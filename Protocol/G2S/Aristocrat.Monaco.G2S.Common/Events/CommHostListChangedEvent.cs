namespace Aristocrat.Monaco.G2S.Common.Events
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     A CommHostListChangedEvent posted whenever there are any changes to the elements or attributes within the list of
    ///     registered hosts or the list of devices assigned to the hosts.
    /// </summary>
    public class CommHostListChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostListChangedEvent" /> class.
        /// </summary>
        /// <param name="hostIndexes">The host indexes of host items were changed.</param>
        public CommHostListChangedEvent(IEnumerable<int> hostIndexes)
        {
            HostIndexes = hostIndexes;
        }

        /// <summary>
        ///     Gets or sets a host indexes of host items were changed.
        /// </summary>
        public IEnumerable<int> HostIndexes { get; set; }
    }
}