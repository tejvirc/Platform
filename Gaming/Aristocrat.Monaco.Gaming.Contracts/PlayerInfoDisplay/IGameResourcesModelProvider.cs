namespace Aristocrat.Monaco.Gaming.Contracts.PlayerInfoDisplay
{
    /// <summary>
    ///     Finds and parse Game details to get resource asset for PID screens
    /// </summary>
    public interface IGameResourcesModelProvider
    {
        /// <summary>
        ///     Finds and parse Game details to get resource asset for PID screens
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        IPlayInfoDisplayResourcesModel Find(int gameId);
    }
}