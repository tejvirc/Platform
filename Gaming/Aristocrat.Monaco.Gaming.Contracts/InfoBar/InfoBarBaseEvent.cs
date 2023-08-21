namespace Aristocrat.Monaco.Gaming.Contracts.InfoBar
{
    using Cabinet.Contracts;
    using Kernel;

    /// <summary>
    ///     Base event from which all InfoBar events derive
    /// </summary>
    /// <seealso cref="BaseEvent" />
    public abstract class InfoBarBaseEvent : BaseEvent
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoBarBaseEvent"/> class.
        /// </summary>
        /// <param name="displayTarget">The display target.</param>
        protected InfoBarBaseEvent(DisplayRole displayTarget)
        {
            DisplayTarget = displayTarget;
        }

        /// <summary>
        ///     The display which this event is targeted at
        /// </summary>
        public DisplayRole DisplayTarget { get; set; }
    }
}