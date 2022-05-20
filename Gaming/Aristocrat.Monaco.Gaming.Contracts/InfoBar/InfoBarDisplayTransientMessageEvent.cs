namespace Aristocrat.Monaco.Gaming.Contracts.InfoBar
{
    using System;
    using Cabinet.Contracts;

    /// <summary>
    ///     Causes the InfoBar to become visible and display a transitory message with a timeout
    /// </summary>
    /// <remarks>
    ///     Supports conformity to G2S Message Protocol v3.0, Appendix E, Section 4.2
    /// </remarks>
    /// <seealso cref="InfoBarDisplayMessageBaseEvent" />
    public class InfoBarDisplayTransientMessageEvent : InfoBarDisplayMessageBaseEvent
    {
        /// <summary>
        ///     The default message display duration
        /// </summary>
        public static readonly TimeSpan DefaultMessageDisplayDuration = TimeSpan.FromSeconds(10);

        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoBarDisplayTransientMessageEvent" /> class.
        /// </summary>
        /// <param name="ownerId">The ID of the message owner.</param>
        /// <param name="message">The message to display on the InfoBar.</param>
        /// <param name="duration">The duration to display the message for.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backgroundColor">Color of the bar background.</param>
        /// <param name="region">The region.</param>
        /// <param name="displayTarget">The display target.</param>
        public InfoBarDisplayTransientMessageEvent(
            Guid ownerId,
            string message,
            TimeSpan duration,
            InfoBarColor textColor,
            InfoBarColor backgroundColor,
            InfoBarRegion region,
            DisplayRole displayTarget)
            : base(ownerId, message, textColor, backgroundColor, region, displayTarget)
        {
            Duration = duration;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoBarDisplayTransientMessageEvent" /> class, using the default
        ///     message display duration.
        /// </summary>
        /// <param name="ownerId">The ID of the message owner.</param>
        /// <param name="message">The message to display on the InfoBar.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backgroundColor">Color of the bar background.</param>
        /// <param name="region">The region.</param>
        /// <param name="displayTarget">The display which this event is targeted at</param>
        public InfoBarDisplayTransientMessageEvent(
            Guid ownerId,
            string message,
            InfoBarColor textColor,
            InfoBarColor backgroundColor,
            InfoBarRegion region,
            DisplayRole displayTarget)
            : this(ownerId, message, DefaultMessageDisplayDuration, textColor, backgroundColor, region, displayTarget)
        {
        }

        /// <summary>Gets the duration.</summary>
        /// <value>The duration the message should be visible for.</value>
        public TimeSpan Duration { get; }
    }
}