namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Timers;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts;
    using Hardware.Contracts.Battery;
    using Hardware.Contracts.Door;
    using Kernel;
    using log4net;
    using Kernel.MessageDisplay;
    using Monaco.Localization.Properties;
    using Kernel.Contracts.MessageDisplay;
    using Timer = System.Timers.Timer;
    
    public class BatteryMonitor : IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMessageDisplay _messageDisplay;
        private readonly IBattery _batteryTest;
        private readonly IMeterManager _meterManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IEventBus _eventBus;
        private readonly double _pollingInterval = TimeSpan.FromHours(24).TotalMilliseconds;

        private Timer _timer;
        private Battery _battery1, _battery2;

        private bool _disposed;

        public BatteryMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IMessageDisplay>(),
                ServiceManager.GetInstance().GetService<IBattery>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public BatteryMonitor(
            IMessageDisplay messageDisplay,
            IBattery batteryTest,
            IMeterManager meterManager,
            IPropertiesManager propertiesManager,
            IEventBus eventBus)
        {
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _batteryTest = batteryTest ?? throw new ArgumentNullException(nameof(batteryTest));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => nameof(BatteryMonitor);

        public ICollection<Type> ServiceTypes => new[] { typeof(BatteryMonitor) };

        public void Initialize()
        {
            Logger.Info("Initializing BatteryMonitor...");

            _battery1 = new Battery(
                0,
                HardwareConstants.Battery1Low,
                ResourceKeys.BackupBattery1Low,
                ApplicationConstants.Battery1Guid);
            _battery2 = new Battery(
                1,
                HardwareConstants.Battery2Low,
                ResourceKeys.BackupBattery2Low,
                ApplicationConstants.Battery2Guid);

            _eventBus.Subscribe<OpenEvent>(this, HandleOpenEvent);

            SetTimer();
            PollBatteries(_timer, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer.Dispose();
            }

            _disposed = true;
        }

        private void HandleOpenEvent(OpenEvent openEvent)
        {
            if (openEvent.LogicalId == (int)DoorLogicalId.Main)
            {
                PollBatteries(_timer, null);
            }
        }

        private void SetTimer()
        {
            _timer = new Timer(_pollingInterval);
            _timer.Elapsed += PollBatteries;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void PollBatteries(object source, ElapsedEventArgs e)
        {
            var (battery1Result, battery2Result) = _batteryTest.Test();

            HandleBatteryTestResult(_battery1, battery1Result);
            HandleBatteryTestResult(_battery2, battery2Result);
        }

        private void HandleBatteryTestResult(Battery battery, bool testResult)
        {
            var batteryStatus = _propertiesManager.GetValue(battery.StatusKey, true);

            if (!testResult)
            {
                Logger.Info("Battery " + battery.Index + " status is low");
                _messageDisplay.DisplayMessage(
                    new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(battery.ErrorMessageKey),
                        DisplayableMessageClassification.SoftError,
                        DisplayableMessagePriority.Normal,
                        battery.Guid));
                _eventBus.Publish(new BatteryLowEvent(battery.Index));

                if (batteryStatus)
                {
                    Logger.Info("Increment battery " + battery.Index + " low count");
                    _meterManager.GetMeter(ApplicationMeters.BatteryLowCount).Increment(1);
                }
            }
            else
            {
                Logger.Info("Battery " + battery.Index + " status is good");
                _messageDisplay.RemoveMessage(battery.Guid);
            }

            _propertiesManager.SetProperty(battery.StatusKey, testResult);
        }

        private class Battery
        {
            public Battery(int index, string statusKey, string errorMessageKey, Guid guid)
            {
                Index = index;
                StatusKey = statusKey;
                ErrorMessageKey = errorMessageKey;
                Guid = guid;
            }

            public int Index { get; }

            public string StatusKey { get; }

            public string ErrorMessageKey { get; }

            public Guid Guid { get; }
        }
    }
}