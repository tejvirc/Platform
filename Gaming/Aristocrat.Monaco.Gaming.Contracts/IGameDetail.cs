namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to get the game details
    /// </summary>
    public interface IGameDetail :
        IGameProfile,
        IGameAttributes
    {
        /// <summary>
        ///     Gets the value of each credit wagered as part of the game
        /// </summary>
        IEnumerable<long> ActiveDenominations { get; }

        /// <summary>
        ///     Gets the list of all supported denominations for each credit wagered as part of the game
        /// </summary>
        IEnumerable<long> SupportedDenominations { get; }

        /// <summary>
        ///     Gets the list of all supported denominations for each credit wagered as part of the game
        /// </summary>
        IEnumerable<IDenomination> Denominations { get; }

        /// <summary>
        /// Gets the list of all the available sub games
        /// </summary>
        IEnumerable<ISubGameDetails> SupportedSubGames { get; }

        /// <summary>
        /// Gets a list of the active sub games
        /// </summary>
        IEnumerable<ISubGameDetails> ActiveSubGames { get; }
    }
}
