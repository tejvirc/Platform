namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Operations;
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class OperatingHoursOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly StateChecker _sc;
        private readonly RobotLogger _logger;
        private readonly IPropertiesManager _pm;
        private readonly RobotController _robotController;
        private Timer _operatingHoursTimer;
        private bool _disposed;

        public OperatingHoursOperations(IEventBus eventBus, RobotLogger logger, StateChecker sc, IPropertiesManager pm, RobotController robotController)
        {
            _sc = sc;
            _logger = logger;
            _eventBus = eventBus;
            _pm = pm;
            _robotController = robotController;
        }

        ~OperatingHoursOperations() => Dispose(false);

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
            _logger.Info("OperatingHoursOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();
            if (_robotController.Config.Active.IntervalSetOperatingHours == 0)
            {
                return;
            }
            _operatingHoursTimer = new Timer(
                               (sender) =>
                               {
                                   SetOperatingHours();
                               },
                               null,
                               _robotController.Config.Active.IntervalSetOperatingHours,
                               _robotController.Config.Active.IntervalSetOperatingHours);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _operatingHoursTimer?.Dispose();
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
                if (_operatingHoursTimer is not null)
                {
                    _operatingHoursTimer.Dispose();
                }
                _operatingHoursTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<OperatingHoursEnabledEvent>(
                this,
                _ =>
                {
                    _robotController.InProgressRequests.TryRemove(RobotStateAndOperations.OperatingHoursOperation);
                });
            _eventBus.Subscribe<OperatingHoursExpiredEvent>(
                this,
                _ =>
                {
                    _robotController.InProgressRequests.TryAdd(RobotStateAndOperations.OperatingHoursOperation);
                });
        }

        private bool IsValid()
        {
            var isBlocked = Helper.IsBlockedByOtherOperation(_robotController, new List<RobotStateAndOperations>());
            return !isBlocked && (_sc.IsChooser || (_sc.IsGame && !_sc.IsGameLoading));
        }

        private void SetOperatingHours()
        {
            if (!IsValid())
            {
                return;
            }

            _logger.Info($"Setting operating hours to timeout in 3 seconds for {_robotController.Config.Active.OperatingHoursDisabledDuration} milliseconds", GetType().Name);

            DateTime soon = DateTime.Now.AddSeconds(3);

            DateTime then = soon.AddMilliseconds(Math.Max(_robotController.Config.Active.OperatingHoursDisabledDuration, 100));

            List<OperatingHours> updatedOperatingHours = new List<OperatingHours>()
            {
                new OperatingHours {Day = soon.DayOfWeek, Enabled = false, Time = (int)soon.TimeOfDay.TotalMilliseconds },
                new OperatingHours {Day = then.DayOfWeek, Enabled = true, Time = (int)then.TimeOfDay.TotalMilliseconds }
            };
            _pm.SetProperty(ApplicationConstants.OperatingHours, updatedOperatingHours);
        }
    }
}
