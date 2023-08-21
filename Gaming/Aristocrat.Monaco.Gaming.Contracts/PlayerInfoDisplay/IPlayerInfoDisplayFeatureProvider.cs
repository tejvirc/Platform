namespace Aristocrat.Monaco.Gaming.Contracts.PlayerInfoDisplay
{
    /// <summary>
    ///     Possible feature for Player Info Display
    /// </summary>
    public interface IPlayerInfoDisplayFeatureProvider
    {
        /// <summary>
        ///     Player Info Display is supported
        /// </summary>
        bool IsPlayerInfoDisplaySupported { get; }

        /// <summary>
        ///     Player Info Display supports Game Info screen
        /// </summary>
        bool IsGameInfoSupported { get; }

        /// <summary>
        ///     Player Info Display supports Game Rules screen
        /// </summary>
        /// <returns></returns>
        bool IsGameRulesSupported { get; }

        /// <summary>
        ///     Active game local
        /// </summary>
        string ActiveLocaleCode { get; }

        /// <summary>
        ///     Automatic timeout to exit Player Information screen
        /// </summary>
        int TimeoutMilliseconds { get; }
    }
}