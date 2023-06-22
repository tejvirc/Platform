namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class RebootRequestOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly LobbyStateChecker _stateChecker;
        private readonly RobotLogger _logger;
        private readonly RobotController _robotController;
        private Timer _rebootTimer;
        private Timer _softRebootTimer;
        private bool _disposed;

        public RebootRequestOperations(IEventBus eventBus, RobotLogger logger, LobbyStateChecker sc, RobotController robotController)
        {
            _stateChecker = sc;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
        }

        ~RebootRequestOperations() => Dispose(false);

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
            _logger.Info("RebootRequestOperations Has Been Initiated!", GetType().Name);
            _rebootTimer = new Timer(
                               (sender) =>
                               {
                                   RequestHardReboot();
                               },
                               null,
                               _robotController.Config.ActiveGameMode.IntervalRebootMachine,
                               _robotController.Config.ActiveGameMode.IntervalRebootMachine);

            _softRebootTimer = new Timer(
                               (sender) =>
                               {
                                   RequestSoftReboot();
                               },
                               null,
                               _robotController.Config.ActiveGameMode.IntervalSoftReboot,
                               _robotController.Config.ActiveGameMode.IntervalSoftReboot);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _rebootTimer?.Dispose();
            _softRebootTimer?.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_softRebootTimer is not null)
                {
                    _softRebootTimer.Dispose();
                }
                _softRebootTimer = null;
                if (_rebootTimer is not null)
                {
                    _rebootTimer.Dispose();
                }
                _rebootTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void RequestSoftReboot()
        {
            if (!IsSoftRebootValid())
            {
                return;
            }
            _logger.Info("RequestSoftReboot Received!", GetType().Name);
            _eventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
        }

        private void RequestHardReboot()
        {
            if (!IsRebootValid())
            {
                return;
            }
            _logger.Info("RequestHardReboot Received!", GetType().Name);
            OSManager.ResetComputer();
        }

        private bool IsSoftRebootValid()
        {
            return _stateChecker.IsChooser || _stateChecker.IsLobbyStateGame;
        }

        private bool IsRebootValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _stateChecker.IsInRecovery && (_stateChecker.IsChooser || _stateChecker.IsLobbyStateGame);
        }
    }
}
