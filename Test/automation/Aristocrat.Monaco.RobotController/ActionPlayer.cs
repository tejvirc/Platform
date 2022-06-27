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
        private readonly Dictionary<Actions,Action<Random>> _actionPlayerFunctions;
        private readonly ILog _logger;
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly StateChecker _sc;
        private Timer _ActionPlayerTimer;
        private bool _disposed;
        private bool _enabled;
        private static ActionPlayer instance = null;
        private static readonly object padlock = new object();
        public static ActionPlayer Instatiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new ActionPlayer(robotInfo);
                }
                return instance;
            }
        }
        private ActionPlayer(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _automator = robotInfo.Automator;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
            _actionPlayerFunctions = new Dictionary<Actions, Action<Random>>();
            InitializeActionPlayer();
        }
        ~ActionPlayer() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _ActionPlayerTimer = new Timer(
                               (sender) =>
                               {
                                   if (!_enabled || !IsValid()) { return; }
                                   _eventBus.Publish(new ActionPlayerEvent());
                               },
                               null,
                               1000,
                               _config.Active.IntervalAction);
            _enabled = true;
        }
        public void Halt()
        {
            _enabled = false;
            _ActionPlayerTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ActionPlayerEvent>(this, HandleEvent);
        }

        private void HandleEvent(ActionPlayerEvent obj)
        {
            if (!IsValid())
            {
                //Todo: Log Something
                return;
            }
            var Rng = new Random((int)DateTime.Now.Ticks);
            var action =
                _config.CurrentGameProfile.RobotActions.ElementAt(Rng.Next(_config.CurrentGameProfile.RobotActions.Count));
            _actionPlayerFunctions[action](Rng);
        }

        private bool IsValid()
        {
            return _sc.IsGame;
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
    }
}
