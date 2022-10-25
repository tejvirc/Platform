namespace Aristocrat.Monaco.Kernel.Contracts
{
    using System;

    /// <summary>
    ///     This event signals that something within the system has requested an exit
    /// </summary>
    public class ExitRequestedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExitRequestedEvent" /> class.
        ///     Also, the constructor will set the Application ExitCode so keep this in mind.
        /// </summary>
        /// <param name="exitAction">The requested exit action</param>
        public ExitRequestedEvent(ExitAction exitAction)
        {
            ExitAction = exitAction;

            // This line is the source of truth for determining the exit code.
            // As the application closes, the Exit Code stored in Environment.ExitCode
            // will be used as the ExitCode returned from the Application.
            Environment.ExitCode = (int)ExitActionToExitCodeMapper.Map(exitAction);
        }

        /// <summary>
        ///     Gets the <see cref="ExitAction" />
        /// </summary>
        public ExitAction ExitAction { get; }
    }
}