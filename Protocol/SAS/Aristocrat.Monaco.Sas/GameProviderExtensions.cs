namespace Aristocrat.Monaco.Sas
{
    using System.Linq;
    using Gaming.Contracts;

    /// <summary>
    ///     Extension methods for <see cref="IGameProvider"/>
    /// </summary>
    public static class GameProviderExtensions
    {
        /// <summary>
        ///     Gets the game and denom Id for the SAS game id
        /// </summary>
        /// <param name="provider">An instance of <see cref="IGameProvider"/></param>
        /// <param name="sasGameId">The game id reported to SAS</param>
        /// <returns>The game and denom for this game id</returns>
        public static (IGameDetail game, IDenomination denom) GetGameDetail(this IGameProvider provider, long sasGameId)
        {
            return provider.GetAllGames().SelectMany(game => game.Denominations.Select(denom => (game, denom)))
                .SingleOrDefault(x => x.denom.Id == sasGameId);
        }

        /// <summary>
        ///     Get the SAS game id for the provided game id and denom
        /// </summary>
        /// <param name="provider">An instance of <see cref="IGameProvider"/></param>
        /// <param name="gameId">The game id to get the SAS game Id</param>
        /// <param name="denom">The denom to get the SAS game id</param>
        /// <returns>The SAS game ID or null if one is not found</returns>
        public static long? GetGameId(this IGameProvider provider, int gameId, long denom)
        {
            return provider.GetGame(gameId)?.Denominations.SingleOrDefault(x => x.Value == denom)?.Id;
        }
    }
}