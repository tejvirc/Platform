namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Operations;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class OperatingHoursOperations : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly StateChecker _sc;
        private readonly ILog _logger;
        private readonly Automation _automator;
        private IPropertiesManager _pm;
        private Timer _OperatingHoursTimer;
        private bool _disposed;
        public OperatingHoursOperations(RobotInfo robotInfo)
        {
            _eventBus = robotInfo.EventBus;
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _logger = robotInfo.Logger;
            _automator = robotInfo.Automator;
            _pm = robotInfo.PropertiesManager;
        }
        ~OperatingHoursOperations() => Dispose(false);
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
                _OperatingHoursTimer?.Dispose();
            }
            _disposed = true;
        }
        public void Execute()
        {
            SubscribeToEvents();
            if (_config.Active.IntervalSetOperatingHours == 0) { return; }
            _OperatingHoursTimer = new Timer(
                               (sender) =>
                               {
                                   SetOperatingHours();
                               },
                               null,
                               _config.Active.IntervalSetOperatingHours,
                               _config.Active.IntervalSetOperatingHours);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<OperatingHoursEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatingHoursEnabledEvent>(this, _ =>
            {
                //log
                _eventBus.Publish(new GameLoadRequestEvent());
            });
        }
        private void HandleEvent(OperatingHoursEvent obj)
        {
            SetOperatingHours();
        }
        private bool IsValid()
        {
            return !_sc.IsInRecovery;
        }
        public void Halt()
        {
            _OperatingHoursTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }
        private void SetOperatingHours()
        {
            if (!IsValid()) { return; }
            
            _logger.Info($"Setting operating hours to timeout in 3 seconds for {_config.Active.OperatingHoursDisabledDuration} milliseconds");

            DateTime soon = DateTime.Now.AddSeconds(3);

            DateTime then = soon.AddMilliseconds(Math.Max(_config.Active.OperatingHoursDisabledDuration, 100));

            List<OperatingHours> updatedOperatingHours = new List<OperatingHours>()
            {
                new OperatingHours {Day = soon.DayOfWeek, Enabled = false, Time = (int)soon.TimeOfDay.TotalMilliseconds },
                new OperatingHours {Day = then.DayOfWeek, Enabled = true, Time = (int)then.TimeOfDay.TotalMilliseconds }
            };
            _pm.SetProperty(ApplicationConstants.OperatingHours, updatedOperatingHours);
            _eventBus.Publish(new PropertyChangedEvent(ApplicationConstants.OperatingHours));
        }
    }
}
