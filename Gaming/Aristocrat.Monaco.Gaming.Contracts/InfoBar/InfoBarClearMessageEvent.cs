namespace Aristocrat.Monaco.Gaming.Contracts.InfoBar
{
    using System;
    using Cabinet.Contracts;

    /// <summary>
    ///     Clears the messages from the specified InfoBar regions
    /// </summary>
    /// <seealso cref="InfoBarBaseEvent" />
    public class InfoBarClearMessageEvent : InfoBarBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoBarClearMessageEvent" /> class.
        /// </summary>
        /// <param name="ownerId">The owner Id.</param>
        /// <param name="displayTarget">The display target.</param>
        /// <param name="regions">The region to clear.</param>
        public InfoBarClearMessageEvent(Guid ownerId, DisplayRole displayTarget, params InfoBarRegion[] regions)
            : base(displayTarget)
        {
            OwnerId = ownerId;
            Regions = regions;
        }

        /// <summary>Gets the unique ID of the message owner.</summary>
        /// <value>The unique ID of the message owner.</value>
        public Guid OwnerId { get; }

        /// <summary>Gets the regions to clear</summary>
        public InfoBarRegion[] Regions { get; }
    }
}