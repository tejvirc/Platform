namespace Aristocrat.Monaco.Gaming.Contracts.Lobby
{
    /// <summary>
    ///     Lobby Constants
    /// </summary>
    public static class LobbyConstants
    {
        /// <summary>
        ///     The responsible gaming time limits
        /// </summary>
        public const string RGTimeLimitsInMinutes = @"Lobby.RGTimeLimitsInMinutes";

        /// <summary>
        ///     The responsible gaming time limits
        /// </summary>
        public const string RGPlayBreaksInMinutes = @"Lobby.RGPlayBreaksInMinutes";

        /// <summary>
        ///     Key for the lobby play time remaining property (we have to persist this).
        /// </summary>
        public const string LobbyPlayTimeRemainingInSeconds = "Lobby.PlayTimeRemainingInSeconds";

        /// <summary>
        ///     Key for the lobby play time elapsed property (we have to persist this).
        /// </summary>
        public const string LobbyPlayTimeElapsedInSeconds = "Lobby.PlayTimeElapsedInSeconds";

        /// <summary>
        ///     Key for the override for the lobby play time elapsed property.
        /// </summary>
        public const string LobbyPlayTimeElapsedInSecondsOverride = "Lobby.PlayTimeElapsedInSecondsOverride";

        /// <summary>
        ///     Key for the lobby play time dialog timeout property
        /// </summary>
        public const string LobbyPlayTimeDialogTimeoutInSeconds = "Lobby.PlayTimeDialogTimeoutInSeconds";

        /// <summary>
        ///     Key for the lobby play time session count (we have to persist this).
        /// </summary>
        public const string LobbyPlayTimeSessionCount = "Lobby.PlayTimeSessionCount";

        /// <summary>
        ///     Key for the override for the lobby play time elapsed property.
        /// </summary>
        public const string LobbyPlayTimeSessionCountOverride = "Lobby.PlayTimeSessionCountOverride";

        /// <summary>
        ///     Key for the lobby IsTimeLimitDlgVisible property (we have to persist this).
        /// </summary>
        public const string LobbyIsTimeLimitDlgVisible = "Lobby.IsTimeLimitDlgVisible";

        /// <summary>
        ///     Key for the lobby ShowTimeLimitDlgPending property (we have to persist this).
        /// </summary>
        public const string LobbyShowTimeLimitDlgPending = "Lobby.ShowTimeLimitDlgPending";

        /// <summary>
        ///     Key for ResponsibleGaming SessionPlayBreakTime property (we have to persist this).
        /// </summary>
        public const string ResponsibleGamingPlayBreakTimeoutInSeconds = "Lobby.RGPlayBreakTimeoutInSeconds";

        /// <summary>
        ///     Key for ResponsibleGaming SessionPlayBreakHit property (we have to persist this).
        /// </summary>
        public const string ResponsibleGamingPlayBreakHit = "Lobby.RGPlayBreakHit";

        /// <summary>
        ///     Key for lobby game ordering
        /// </summary>
        public const string DefaultGameDisplayOrderByThemeId = @"Lobby.DefaultGameDisplayOrderByThemeId";
    }
}
