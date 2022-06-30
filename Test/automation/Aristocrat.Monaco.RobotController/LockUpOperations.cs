namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Threading;

    internal class LockUpOperations : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly StateChecker _sc;
        private readonly ILog _logger;
        private readonly Automation _automator;
        private readonly Action _disableRobotController;
        private bool _disposed;
        private Timer _RebootTimer;
        private Timer _lockupTimer;
        public LockUpOperations(RobotInfo robotInfo)
        {
            _eventBus = robotInfo.EventBus;
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _logger = robotInfo.Logger;
            _automator = robotInfo.Automator;
            _disableRobotController = robotInfo.DisableRobotController;
        }
        ~LockUpOperations() => Dispose(false);
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
                _eventBus.UnsubscribeAll(this);
                _RebootTimer?.Dispose();
                _lockupTimer?.Dispose();
            }
            _disposed = true;
        }
        public void Execute()
        {
            SubscribeToEvents();
            if (_config.Active.IntervalTriggerLockup == 0) { return; }
            _lockupTimer = new Timer(
                               (sender) =>
                               {
                                   RequestLockUp();
                               },
                               null,
                               _config.Active.IntervalTriggerLockup,
                               _config.Active.IntervalTriggerLockup);
        }

        private void RequestLockUp()
        {
            if (!IsValid()) { return; }
            _automator.EnterLockup();
            _lockupTimer = new Timer(
            (sender) =>
            {
                _automator.ExitLockup();
                _lockupTimer.Dispose();
            }, null, Constants.LockupDuration, System.Threading.Timeout.Infinite);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<LockUpRequestEvent>(this, HandleEvent);

            _eventBus.Subscribe<SystemDisableAddedEvent>(
                           this,
                           evt =>
                           {
                               //We could have exited operating hours and an attempt to insert credits may occur
                               //inspect the previous controller state which should be set in the BalanceCheck call
                               if (evt.DisableReasons == "Outside Hours of Operation")
                               {
                                   _logger.Info("Not disabling because disable for Operating Hours is expected.");
                                   return;
                               }

                               if (evt.DisableReasons.Contains("Disabled by the voucher"))
                               {
                                   _logger.Info("Not disabling for voucher device");
                                   return;
                               }

                               if (evt.DisableReasons.Contains("Game Play Request Failed"))
                               {
                                   _logger.Info("Not disabling for game play request failed");
                                   return;
                               }

                               if (evt.DisableReasons.Contains("Central Server Offline"))
                               {
                                   _logger.Info("Not disabling for central server offline");
                                   return;
                               }

                               if (evt.DisableReasons.Contains("Protocol Initialization In Progress"))
                               {
                                   _logger.Info("Not disabling for protocol initialization");
                                   return;
                               }

                               if (evt.DisableId == ApplicationConstants.LiveAuthenticationDisableKey)
                               {
                                   _logger.Info("Not disabling for signature verification");
                                   return;
                               }
                               if (_config.Active.DisableOnLockup)
                               {
                                   _logger.Info($"Disabling for system disable {evt.DisableId}, reason: {evt.DisableReasons}");
                               }
                           });
        }
        private void HandleEvent(LockUpRequestEvent obj)
        {
            RequestLockUp();
        }
        private bool IsValid()
        {
            return _sc.IsChooser;
        }
        public void Halt()
        {
            _RebootTimer?.Dispose();
            _lockupTimer?.Dispose();
        }
    }
}
