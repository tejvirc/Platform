namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Posted to indicate a game has been selected for diagnostics.
    /// </summary>
    [Serializable]
    public class GameDiagnosticsStartedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDiagnosticsStartedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The gameId.</param>
        /// <param name="denomId">The denomination for the game being replayed.</param>
        /// <param name="context">The context associates with the event</param>
        /// <param name="label">The label for the game being replayed</param>
        public GameDiagnosticsStartedEvent(int gameId, long denomId, string label, IDiagnosticContext context)
        {
            GameId = gameId;
            Denomination = denomId;
            Label = label;
            Context = context;
        }

        /// <summary>
        ///     Gets the game id
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     Gets the label for the game being replayed.
        /// </summary>
        public string Label { get; }

        /// <summary>
        ///     Gets the denomination for the game being replayed.
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        ///     Gets the context associated with the event
        /// </summary>
        public IDiagnosticContext Context { get; }
    }
}