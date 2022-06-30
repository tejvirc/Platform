namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class PlayerOperations : IRobotOperations, IDisposable
    {
        private readonly Configuration _config;
        private readonly Dictionary<Actions,Action<Random>> _actionPlayerFunctions;
        private readonly ILog _logger;
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly StateChecker _sc;
        private Timer _ActionPlayerTimer;
        private bool _disposed;
        private static PlayerOperations instance = null;
        private static readonly object padlock = new object();
        public static PlayerOperations Instantiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new PlayerOperations(robotInfo);
                }
                return instance;
            }
        }
        private PlayerOperations(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _automator = robotInfo.Automator;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
            _actionPlayerFunctions = new Dictionary<Actions, Action<Random>>();
            InitializeActionPlayer();
        }
        ~PlayerOperations() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _ActionPlayerTimer = new Timer(
                               (sender) =>
                               {
                                   RequestPlay();
                               },
                               null,
                               _config.Active.IntervalAction,
                               _config.Active.IntervalAction);
        }
        private void RequestPlay()
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
        public void Halt()
        {
            _ActionPlayerTimer?.Dispose();
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<PlayRequestEvent>(this, HandleEvent);
        }
        private void HandleEvent(PlayRequestEvent obj)
        {
            RequestPlay();
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
