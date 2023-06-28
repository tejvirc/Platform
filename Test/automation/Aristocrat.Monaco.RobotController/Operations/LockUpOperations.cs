namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class LockUpOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly StateChecker _stateChecker;
        private readonly RobotLogger _logger;
        private readonly Automation _automator;
        private readonly RobotController _robotController;
        private bool _disposed;
        private Timer _lockupTimer;

        public LockUpOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, RobotController robotController)
        {
            _stateChecker = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
        }

        ~LockUpOperations() => Dispose(false);

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
            _logger.Info("LockUpOperations Has Been Initiated!", GetType().Name);
            if (_robotController.Config.Active.IntervalTriggerLockup == 0)
            {
                return;
            }
            _lockupTimer = new Timer(
                               (sender) =>
                               {
                                   RequestLockUp();
                               },
                               null,
                               _robotController.Config.Active.IntervalTriggerLockup,
                               _robotController.Config.Active.IntervalTriggerLockup);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _automator.ExitLockup();
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_lockupTimer is not null)
                {
                    _lockupTimer.Dispose();
                }
                _lockupTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void RequestLockUp()
        {
            if (!IsValid())
            {
                return;
            }
            _robotController.BlockOtherOperations(RobotStateAndOperations.LockUpOperation);
            _logger.Info("RequestLockUp Received!", GetType().Name);
            _automator.EnterLockup();

            var lockupDuration = TimeSpan.FromMilliseconds(Constants.LockupDuration);
            Task.Delay(lockupDuration).ContinueWith(task =>
            {
                _logger.Info("RequestExitLockup Received!", GetType().Name);
                _automator.ExitLockup();
                _robotController.UnBlockOtherOperations(RobotStateAndOperations.LockUpOperation);
            });
        }

        private bool IsValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return isBlocked && _stateChecker.IsChooser;
        }
    }
}
