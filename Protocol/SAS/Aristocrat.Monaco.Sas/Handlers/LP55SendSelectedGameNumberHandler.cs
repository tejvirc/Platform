namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     The LP 55 Send Selected Game Number Handler
    /// </summary>
    public class LP55SendSelectedGameNumberHandler : ISasLongPollHandler<LongPollReadSingleValueResponse<int>, LongPollData>
    {
        private const int DefaultSelectedGame = 0;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Creates the LP55SendSelectedGameNumberHandler instance
        /// </summary>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="gameProvider">An instance of <see cref="IGameProvider"/></param>
        public LP55SendSelectedGameNumberHandler(IPropertiesManager propertiesManager, IGameProvider gameProvider)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendSelectedGameNumber
        };

        /// <inheritdoc />
        public LongPollReadSingleValueResponse<int> Handle(LongPollData data)
        {
            var isGameRunning = (bool)_propertiesManager.GetProperty(GamingConstants.IsGameRunning, false);
            if (!isGameRunning)
            {
                return new LongPollReadSingleValueResponse<int>(DefaultSelectedGame);
            }

            var gameId = _propertiesManager.GetValue(GamingConstants.SelectedGameId, 0);
            var denom = _propertiesManager.GetValue(GamingConstants.SelectedDenom, 0L);
            var sasGameId = (int)(_gameProvider.GetGameId(gameId, denom) ?? DefaultSelectedGame);
            return new LongPollReadSingleValueResponse<int>(sasGameId);
        }
    }
}