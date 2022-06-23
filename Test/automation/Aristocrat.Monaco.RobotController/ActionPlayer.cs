namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class ActionPlayer : IRobotOperations, IDisposable
    {
        private readonly Configuration _config;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly Dictionary<Actions,Action<Random>> _actionPlayerFunctions;
        private readonly ILog _logger;
        private readonly IEventBus _eventBus;
        private Automation _automator;
        private Timer _ActionPlayerTimer;
        private bool _disposed;

        public ActionPlayer(Configuration config, ILobbyStateManager lobbyStateManager, ILog logger, Automation automator, IEventBus eventBus)
        {
            _config = config;
            _lobbyStateManager = lobbyStateManager;
            _logger = logger;
            _automator = automator;
            _eventBus = eventBus;
            _actionPlayerFunctions = new Dictionary<Actions, Action<Random>>();
            InitializeActionPlayer();
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ActionPlayerEvent>(this, HandleEvent);
        }

        private void HandleEvent(ActionPlayerEvent obj)
        {
            var Rng = new Random((int)DateTime.Now.Ticks);
            var action =
                _config.CurrentGameProfile.RobotActions.ElementAt(Rng.Next(_config.CurrentGameProfile.RobotActions.Count));
            _actionPlayerFunctions[action](Rng);
        }

        ~ActionPlayer()
        {
            Dispose(false);
        }

        private void InitializeActionPlayer()
        {
            _actionPlayerFunctions.Add(Actions.SpinRequest,
            (Rng) =>
            {
                _logger.Info("Spin Request");
                _automator.SpinRequest();
            });

            _actionPlayerFunctions.Add(Actions.BetLevel,
            (Rng) =>
            {
                _logger.Info("Changing bet level");
                //increment/decrement bet level - physical id: 23 - 27
                var betIndices = _config.GetBetIndices();
                var index = Math.Min(betIndices[Rng.Next(betIndices.Count)], 5);
                _automator.SetBetLevel(index);
            });

            _actionPlayerFunctions.Add(Actions.BetMax,
            (Rng) =>
            {
                _logger.Info("Bet Max");
                _automator.SetBetMax();
            });

            _actionPlayerFunctions.Add(Actions.LineLevel,
            (Rng) =>
            {
                _logger.Info("Change Line Level");
                // increment/decrement line level - physical id: 30 - 34
                var lineIndices = _config.GetLineIndices();
                _automator.SetLineLevel(lineIndices[Rng.Next(lineIndices.Count)]);
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _ActionPlayerTimer?.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        public void Execute()
        {
            _ActionPlayerTimer = new Timer(
                               (sender) =>
                               {
                                   if (_lobbyStateManager.CurrentState is LobbyState.Game)
                                   {
                                       BalanceCheck();
                                       _eventBus.Publish(new ActionPlayerEvent());
                                   }
                               },
                               null,
                               _config.Active.IntervalAction,
                               Timeout.Infinite);
        }

        private void BalanceCheck()
        {
            _eventBus.Publish(new BalanceCheckEvent());
        }

        public void Halt()
        {
            _ActionPlayerTimer?.Dispose();
        }
    }
}
