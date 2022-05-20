namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Posted to indicate a game diagnostics has completed.
    /// </summary>
    [Serializable]
    public class GameDiagnosticsCompletedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDiagnosticsCompletedEvent" /> class.
        /// </summary>
        /// <param name="context">The context associates with the event</param>
        public GameDiagnosticsCompletedEvent(IDiagnosticContext context)
        {
            Context = context;
        }

        /// <summary>
        ///     Gets the context associated with the event
        /// </summary>
        public IDiagnosticContext Context { get; }
    }
}