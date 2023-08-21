namespace Aristocrat.Monaco.Gaming.Contracts.InfoBar
{
    using System;
    using Cabinet.Contracts;

    /// <summary>
    ///     Causes the InfoBar to become visible and display a static message
    /// </summary>
    /// <remarks>
    ///     Supports conformity to G2S Message Protocol v3.0, Appendix E, Section 4.2
    /// </remarks>
    /// <seealso cref="InfoBarDisplayMessageBaseEvent" />
    public class InfoBarDisplayStaticMessageEvent : InfoBarDisplayMessageBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoBarDisplayStaticMessageEvent" /> class.
        /// </summary>
        /// <param name="ownerId">The owner ID.</param>
        /// <param name="message">The message to display on the InfoBar.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backgroundColor">Color of the bar background.</param>
        /// <param name="region">The region where the text will be displayed.</param>
        /// <param name="displayTarget">The display which this event is targeted at</param>
        public InfoBarDisplayStaticMessageEvent(
            Guid ownerId,
            string message,
            InfoBarColor textColor,
            InfoBarColor backgroundColor,
            InfoBarRegion region,
            DisplayRole displayTarget)
            : base(ownerId, message, textColor, backgroundColor, region, displayTarget)
        {
        }
    }
}