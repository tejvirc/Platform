namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class PlayerOperations : IRobotOperations, IDisposable
    {
        private readonly Configuration _config;
        private readonly Dictionary<Actions,Action<Random>> _actionPlayerFunctions;
        private readonly RobotLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly StateChecker _sc;
        private Timer _ActionPlayerTimer;
        private bool _disposed;
        public PlayerOperations(IEventBus eventBus, RobotLogger logger, Automation automator, Configuration config, StateChecker sc)
        {
            _config = config;
            _sc = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
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
                _logger.Error("RequestPlay Validation Failed", GetType().Name);
                return;
            }
            _logger.Info("RequestPlay Received!", GetType().Name);
            var Rng = new Random((int)DateTime.Now.Ticks);
            var action =
                _config.CurrentGameProfile.RobotActions.ElementAt(Rng.Next(_config.CurrentGameProfile.RobotActions.Count));
            _actionPlayerFunctions[action](Rng);
        }
        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _ActionPlayerTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
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
                _logger.Info("Spin Request", GetType().Name);
                _automator.SpinRequest();
            });

            _actionPlayerFunctions.Add(Actions.BetLevel,
            (Rng) =>
            {
                _logger.Info("Changing bet level", GetType().Name);
                var betIndices = _config.GetBetIndices();
                var index = Math.Min(betIndices[Rng.Next(betIndices.Count)], 5);
                _automator.SetBetLevel(index);
            });

            _actionPlayerFunctions.Add(Actions.BetMax,
            (Rng) =>
            {
                _logger.Info("Bet Max", GetType().Name);
                _automator.SetBetMax();
            });

            _actionPlayerFunctions.Add(Actions.LineLevel,
            (Rng) =>
            {
                _logger.Info("Change Line Level", GetType().Name);
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
