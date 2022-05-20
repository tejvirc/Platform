namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Provides a service for launching game diagnostics.
    /// </summary>
    public interface IGameDiagnostics
    {
        /// <summary>
        ///     Gets the context for the diagnostics session
        /// </summary>
        IDiagnosticContext Context { get; }

        /// <summary>
        ///     Gets a value indicating whether or not input is allow for the diagnostics session
        /// </summary>
        bool AllowInput { get; }

        /// <summary>
        ///     Gets a value indicating whether we are in game diagnostics.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        ///     Gets or sets the game id of the game we need to relaunch after diagnostics are complete.
        ///     0 if there is no game to reload
        /// </summary>
        int RelaunchGameId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating the diagnostics session supports resuming play
        /// </summary>
        bool AllowResume { get; set; }

        /// <summary>
        ///     Starts a game for diagnostics
        /// </summary>
        /// <param name="gameId">The game id.</param>
        /// <param name="denomId">The denomination for the game to replay.</param>
        /// <param name="label"></param>
        /// <param name="context">The diagnostics context</param>
        /// <param name="allowInput">true if user input is allowed during game diagnostics, else false</param>
        void Start(int gameId, long denomId, string label, IDiagnosticContext context, bool allowInput = false);

        /// <summary>
        ///     Ends game diagnostics
        /// </summary>
        void End();
    }
}