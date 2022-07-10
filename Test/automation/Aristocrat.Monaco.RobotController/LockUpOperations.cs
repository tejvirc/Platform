namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class LockUpOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly StateChecker _sc;
        private readonly RobotLogger _logger;
        private readonly Automation _automator;
        private readonly RobotController _robotController;
        private bool _disposed;
        private Timer _lockupTimer;

        public LockUpOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, RobotController robotController)
        {
            _sc = sc;
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
            _lockupTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
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
            _logger.Info("RequestLockUp Received!", GetType().Name);
            _automator.EnterLockup();
            _robotController.InProgressRequests.TryAdd(RobotStateAndOperations.LockUpOperation);
            _lockupTimer = new Timer(
            (sender) =>
            {
                _logger.Info("RequestExitLockup Received!", GetType().Name);
                _automator.ExitLockup();
                _lockupTimer.Dispose();
                _robotController.InProgressRequests.TryRemove(RobotStateAndOperations.LockUpOperation);
            }, null, Constants.LockupDuration, Timeout.Infinite);
        }

        private bool IsValid()
        {
            var isBlocked = Helper.IsBlockedByOtherOperation(_robotController, new List<RobotStateAndOperations>());
            return !isBlocked && _sc.IsChooser;
        }
    }
}
