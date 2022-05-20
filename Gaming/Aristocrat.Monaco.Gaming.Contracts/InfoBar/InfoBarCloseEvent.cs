namespace Aristocrat.Monaco.Gaming.Contracts.InfoBar
{
    using Cabinet.Contracts;

    /// <summary>
    ///     Signals the InfoBar to close
    /// </summary>
    /// <seealso cref="InfoBarBaseEvent" />
    public class InfoBarCloseEvent : InfoBarBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoBarCloseEvent" /> class.
        /// </summary>
        /// <param name="displayTarget">The display target.</param>
        public InfoBarCloseEvent(DisplayRole displayTarget)
            : base(displayTarget)
        {
        }
    }
}