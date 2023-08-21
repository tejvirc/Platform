namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to retrieve a protocol set of enabled games
    /// </summary>
    public interface IAspGameProvider
    {
        /// <summary>
        ///     Provides a specific set of enabled games for ASP protocol
        /// </summary>
        /// <returns>A List of active denomination in pair with associated game</returns>
        IReadOnlyList<(IGameDetail game, IDenomination denom)> GetEnabledGames();
    }
}
