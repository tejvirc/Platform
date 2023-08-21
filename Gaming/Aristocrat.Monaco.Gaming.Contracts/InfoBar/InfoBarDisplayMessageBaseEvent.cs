namespace Aristocrat.Monaco.Gaming.Contracts.InfoBar
{
    using System;
    using Cabinet.Contracts;

    /// <summary>
    ///     Base class for all InfoBar display message events
    /// </summary>
    /// <seealso cref="InfoBarBaseEvent" />
    public abstract class InfoBarDisplayMessageBaseEvent : InfoBarBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoBarDisplayMessageBaseEvent" /> class.
        /// </summary>
        /// <param name="ownerId">The owner ID.</param>
        /// <param name="message">The message.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <param name="region">The region.</param>
        /// <param name="displayTarget">The display which this event is targeted at</param>
        protected InfoBarDisplayMessageBaseEvent(
            Guid ownerId,
            string message,
            InfoBarColor textColor,
            InfoBarColor backgroundColor,
            InfoBarRegion region,
            DisplayRole displayTarget)
            :base(displayTarget)
        {
            OwnerId = ownerId;
            Message = message;
            TextColor = textColor;
            BackgroundColor = backgroundColor;
            Region = region;
        }

        /// <summary>Gets the unique ID of the message owner.</summary>
        /// <value>The unique ID of the message owner.</value>
        public Guid OwnerId { get; }

        /// <summary>Gets the color of the text.</summary>
        /// <value>The color of the text.</value>
        public InfoBarColor TextColor { get; }

        /// <summary>Gets the region.</summary>
        /// <value>The region text will be displayed in.</value>
        public InfoBarRegion Region { get; }

        /// <summary>Gets the message.</summary>
        /// <value>The message to display.</value>
        public string Message { get; }

        /// <summary>Gets the color of the background.</summary>
        /// <value>The color of the background.</value>
        public InfoBarColor BackgroundColor { get; }
    }
}