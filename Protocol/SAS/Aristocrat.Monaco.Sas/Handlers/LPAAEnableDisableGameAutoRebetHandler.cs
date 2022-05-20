namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Handles the auto play command 
    /// </summary>
    public class LPAAEnableDisableGameAutoRebetHandler :
        ISasLongPollHandler<LongPollReadSingleValueResponse<bool>, LongPollSingleValueData<byte>>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAutoPlayStatusProvider _autoPlayStatusProvider;
        private readonly IGameProvider _gameProvider;
        private readonly ILobbyStateManager _lobbyStateManager;

        /// <summary>
        ///     Creates the LPAAEnableDisableGameAutoRebetHandler instance
        /// </summary>
        public LPAAEnableDisableGameAutoRebetHandler(IPropertiesManager properties, IAutoPlayStatusProvider autoPlayProvider, IGameProvider gameProvider, ILobbyStateManager lobbyStateManager)
        {
            _propertiesManager = properties ?? throw new ArgumentNullException(nameof(properties));
            _autoPlayStatusProvider = autoPlayProvider ?? throw new ArgumentNullException(nameof(autoPlayProvider));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _lobbyStateManager = lobbyStateManager ?? throw new ArgumentNullException(nameof(lobbyStateManager));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.EnableDisableGameAutoRebet };

        /// <inheritdoc/>
        public LongPollReadSingleValueResponse<bool> Handle(LongPollSingleValueData<byte> data)
        {
            var result = new LongPollReadSingleValueResponse<bool>(false);
            var isGameRunning = (bool)_propertiesManager.GetProperty(GamingConstants.IsGameRunning, false);

            if (!isGameRunning || _lobbyStateManager?.CurrentState != LobbyState.Game)
            {
                return result;
            }

            var gameId = _propertiesManager.GetValue(GamingConstants.SelectedGameId, 0);
            var game = _gameProvider.GetGame(gameId);
            var awaitingPlayerSelection = _propertiesManager.GetValue(GamingConstants.AwaitingPlayerSelection, false);

            if (_propertiesManager.GetValue(GamingConstants.AutoPlayAllowed, true) && game.AutoPlaySupported && !awaitingPlayerSelection)
            {
                if (data.Value == (byte)AutoPlay.Start)
                {
                    result.Data = true;
                    _autoPlayStatusProvider.StartSystemAutoPlay();
                }

                if (data.Value == (byte)AutoPlay.Stop)
                {
                    result.Data = true;
                    _autoPlayStatusProvider.StopSystemAutoPlay();
                }
            }

            return result;
        }

        private enum AutoPlay
        {
            Stop = 0,
            Start = 1
        }
    }
}
