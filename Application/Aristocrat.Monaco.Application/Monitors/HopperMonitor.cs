namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Contracts.Extensions;
    using Hardware.Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Hopper;
    using Kernel;
    using static Hardware.Contracts.Hopper.HopperEventsDescriptor;
    using HardwareFaultClearEvent = Hardware.Contracts.Hopper.HardwareFaultClearEvent;
    using HardwareFaultEvent = Hardware.Contracts.Hopper.HardwareFaultEvent;

    /// <summary>
    ///     Handle Lockup events from Hopper.
    /// </summary>
    public sealed class HopperMonitor : GenericBaseMonitor, IService, IDisposable
    {
        private readonly ISystemDisableManager _disableManager;
        private readonly IEventBus _bus;
        private readonly IPropertiesManager _properties;
        private readonly IHopper _hopperService;
        private Alarm _alarm;
        private long _tokenValue;
        private const long DefaultTokenValue = 100000L;

        public string Name => nameof(HopperMonitor);

        public ICollection<Type> ServiceTypes => new[] { typeof(HopperMonitor) };

        public HopperMonitor()
            : this(
                ServiceManager.GetInstance().TryGetService<IHopper>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {

        }

        public HopperMonitor(
            IHopper hopperService,
            ISystemDisableManager disableManager,
            IPropertiesManager propertiesManager,
            IEventBus eventBus)
        {
            _hopperService = hopperService;
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public override string DeviceName => "Hopper";

        public void Initialize()
        {
            if (_hopperService == null) return;
            // These are the sets of errors and states that this class monitors uniquely.
            ManageErrorEnum<HopperFaultTypes>(
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                true);
            SubscribeEvents();
            _alarm = new Alarm();
            _alarm.LoadAlarm();
            _tokenValue = _properties.GetValue(HardwareConstants.CoinValue, DefaultTokenValue);
            var existingFaults = _properties.GetValue(HardwareConstants.HopperFaults, HopperFaultTypes.None);

            //Check the lockups which occured before coming to this point.
            CheckDeviceStatus();

            //Check the lockups which were there before the power cycle.
            CheckSaveDeviceStatus(existingFaults);

            //Checking if Hopper test lockup exist.
            CheckHopperTestLockup();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }
        }

        private void SubscribeEvents()
        {
            _bus.Subscribe<DownEvent>(
                this,
                _ => HardwareFaultClear(),
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);

            _bus.Subscribe<HardwareFaultEvent>(this, Handle);
        }

        private void CheckSaveDeviceStatus(HopperFaultTypes existingFaults)
        {
            foreach (HopperFaultTypes fault in Enum.GetValues(typeof(HopperFaultTypes)))
            {
                if (FaultTexts.ContainsKey(fault) &&
                    existingFaults.HasFlag(fault))
                {
                    HandleLockUp(fault);
                }
            }
        }

        private void Handle(HardwareFaultEvent evt)
        {

            HandleLockUp(evt.Fault);
            PersistLockups();
        }

        private void HandleLockUp(HopperFaultTypes coinFault)
        {
            _alarm.PlayAlarm();
            AddFault(coinFault);

        }

        private void HardwareFaultClear()
        {
            if(_hopperService != null)
            {
                if (_hopperService.Faults != HopperFaultTypes.None)
                {
                    _hopperService.Faults = HopperFaultTypes.None;

                    // Reset hopper.
                    _hopperService.Reset();

                    foreach (HopperFaultTypes fault in Enum.GetValues(typeof(HopperFaultTypes)))
                    {
                        ClearFault(fault);
                    }
                    _properties.SetProperty(HardwareConstants.HopperFaults, HopperFaultTypes.None);
                }
            }

        }
        private void CheckHopperTestLockup()
        {
            if (_properties.GetValue(HardwareConstants.HopperDiagnosticMode, false))
            {
                _disableManager.Disable(HardwareConstants.HopperTestLockKey,
                    SystemDisablePriority.Immediate,
                    () => Hardware.Contracts.Properties.Resources.HopperTestFault,
                    true,
                    () => Hardware.Contracts.Properties.Resources.HopperTestFaultHelp);
            }
        }

        private void CheckDeviceStatus()
        {
            if (_hopperService != null)
            {
                if (_hopperService.Faults != HopperFaultTypes.None)
                {
                    foreach (HopperFaultTypes fault in Enum.GetValues(typeof(HopperFaultTypes)))
                    {
                        if (_hopperService.Faults.HasFlag(fault))
                        {
                            HandleLockUp(fault);
                        }
                    }
                    PersistLockups();
                }
            }
        }

        private void PersistLockups()
        {
            _properties.SetProperty(HardwareConstants.HopperFaults, _hopperService.Faults);
        }
    }
}
