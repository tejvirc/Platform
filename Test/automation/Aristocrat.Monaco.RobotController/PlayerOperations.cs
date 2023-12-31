﻿namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;

    internal class PlayerOperations : IRobotOperations
    {
        private readonly Dictionary<Actions, Action<Random>> _actionPlayerFunctions;
        private readonly RobotLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly StateChecker _stateChecker;
        private readonly RobotController _robotController;
        private Timer _actionPlayerTimer;
        private bool _disposed;

        public PlayerOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, RobotController robotController)
        {
            _stateChecker = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _actionPlayerFunctions = new Dictionary<Actions, Action<Random>>();
            _robotController = robotController;

            InitializeActionPlayer();
        }

        ~PlayerOperations() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            _disposed = false;
        }

        public void Execute()
        {
            _logger.Info("PlayerOperations Has Been Initiated!", GetType().Name);
            _actionPlayerTimer = new Timer(
                               (sender) =>
                               {
                                   RequestPlay();
                               },
                               null,
                               _robotController.Config.Active.IntervalAction,
                               _robotController.Config.Active.IntervalAction);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _actionPlayerTimer?.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_actionPlayerTimer is not null)
                {
                    _actionPlayerTimer.Dispose();
                }
                _actionPlayerTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void RequestPlay()
        {
            if (!IsValid())
            {
                return;
            }
            _logger.Info("RequestPlay Received!", GetType().Name);
            var rng = new Random((int)DateTime.Now.Ticks);


            var action = _robotController.Config.GetRobotActions().GetRandomElement(rng);
            _actionPlayerFunctions[action](rng);
        }

        private bool IsValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _stateChecker.IsGame && !_stateChecker.IsGameLoading;
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
                var betIndices = _robotController.Config.GetBetIndices();
                var index = Math.Min(betIndices[Rng.Next(betIndices.Count)], 5);
                if (index == 1) return; // Input Key 23 is mapped to GameMenu which triggers BeginLobby request
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
                var lineIndices = _robotController.Config.GetLineIndices();
                _automator.SetLineLevel(lineIndices[Rng.Next(lineIndices.Count)]);
            });
        }
    }
}
