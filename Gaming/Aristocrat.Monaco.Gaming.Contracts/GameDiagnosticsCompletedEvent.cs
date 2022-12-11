namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Posted to indicate a game diagnostics has completed.
    /// </summary>
    [ProtoContract]
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
        [ProtoMember(1)]
        public IDiagnosticContext Context { get; }
    }
}