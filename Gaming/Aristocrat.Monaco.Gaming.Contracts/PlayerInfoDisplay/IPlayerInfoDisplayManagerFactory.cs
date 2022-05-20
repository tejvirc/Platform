namespace Aristocrat.Monaco.Gaming.Contracts.PlayerInfoDisplay
{
    /// <summary>
    ///     Provides IPlayerInfoDisplayManager
    /// </summary>
    public interface IPlayerInfoDisplayManagerFactory
    {
        /// <summary>
        ///     Creates and initialises IPlayerInfoDisplayManager
        /// </summary>
        /// <param name="screensProvider">Container which hosts Play Information Screens</param>
        /// <returns></returns>
        IPlayerInfoDisplayManager Create(IPlayerInfoDisplayScreensContainer screensProvider);
    }
}