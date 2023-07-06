namespace Aristocrat.Monaco.Gaming.Contracts.Configuration
{
    /// <summary>
    ///     Allows for the retrieval of denom-restriction params for game packs.
    /// </summary>
    public interface IGamePackConfigurationProvider
    {
        /// <summary>
        ///     Retrieves denomination restrictions for game packs.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <returns></returns>
        object GetDenomRestrictionsByGameId(int gameId);
    }
}