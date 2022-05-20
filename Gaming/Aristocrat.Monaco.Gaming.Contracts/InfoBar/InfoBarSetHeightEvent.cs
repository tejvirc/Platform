namespace Aristocrat.Monaco.Gaming.Contracts.InfoBar
{
    using Cabinet.Contracts;

    /// <summary>
    ///     Set the height of the InfoBar
    /// </summary>
    /// <seealso cref="InfoBarBaseEvent" />
    public class InfoBarSetHeightEvent : InfoBarBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoBarSetHeightEvent" /> class.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="displayTarget">The display which this event is targeted at</param>
        public InfoBarSetHeightEvent(double height, DisplayRole displayTarget)
            : base(displayTarget)
        {
            Height = height;
        }

        /// <summary>Gets the height</summary>
        public double Height { get; }
    }
}