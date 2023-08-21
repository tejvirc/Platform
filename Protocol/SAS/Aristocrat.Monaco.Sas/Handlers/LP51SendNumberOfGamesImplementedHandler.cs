namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles sending number of games implemented
    /// </summary>
    public class LP51SendNumberOfGamesImplementedHandler : ISasLongPollHandler<LongPollReadSingleValueResponse<int>, LongPollData>
    {
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Creates a new instance of the LP51SendNumberOfGamesImplementedHandler
        /// </summary>
        /// <param name="gameProvider">a reference to the game provider</param>
        public LP51SendNumberOfGamesImplementedHandler(IGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.SendNumberOfGames };

        /// <inheritdoc/>
        public LongPollReadSingleValueResponse<int> Handle(LongPollData data)
        {
            return new LongPollReadSingleValueResponse<int>(_gameProvider.GetAllGames().Sum(x => x.Denominations.Count()));
        }
    }
}
