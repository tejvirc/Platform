namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gaming.Contracts;

    /// <summary>
    ///     Contains the information of games which are enabled from Operator Menu Game Configuration changed
    /// </summary>
    public class AspGameProvider : IAspGameProvider
    {
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Initializes an instance of the class.
        /// </summary>
        /// <param name="gameProvider">A <see cref="IGameProvider" /> instance.</param>
        public AspGameProvider(IGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public IReadOnlyList<(IGameDetail game, IDenomination denom)> GetEnabledGames()
        {
            return _gameProvider.GetAllGames()
                .SelectMany(game => game.Denominations.Where(denom => denom.Active).Select(denom => (game, denom)))
                .ToList();
        }
    }
}