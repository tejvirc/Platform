namespace Aristocrat.Monaco.Kernel.Contracts
{
    /// <summary>
    ///     This event signals that something within the system has requested an exit
    /// </summary>
    public class ExitRequestedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExitRequestedEvent" /> class.
        /// </summary>
        /// <param name="exitAction">The requested exit action</param>
        public ExitRequestedEvent(ExitAction exitAction)
        {
            ExitAction = exitAction;
        }

        /// <summary>
        ///     Gets the <see cref="ExitAction" />
        /// </summary>
        public ExitAction ExitAction { get; }
    }
}