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
    public class HopperMonitor : IService, IDisposable
    {
        private readonly ISystemDisableManager _disableManager;
        private readonly IEventBus _bus;
        private readonly IPropertiesManager _properties;
        private readonly IHopper _hopperService;
        private Alarm _alarm;
        private long _tokenValue;

        public string Name => nameof(HopperMonitor);

        public ICollection<Type> ServiceTypes => new[] { typeof(HopperMonitor) };

        public HopperMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IHopper>(),
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
            _hopperService = hopperService ?? throw new ArgumentNullException(nameof(hopperService));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Initialize()
        {
            SubscribeEvents();
            _alarm = new Alarm();
            _alarm.LoadAlarm();
            _tokenValue = _properties.GetValue(HardwareConstants.CoinValue, 100000L);
            CheckLockUp();
        }

        private void SubscribeEvents()
        {
            _bus.Subscribe<DownEvent>(
                this,
                _ => HardwareFaultClear(),
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);

            _bus.Subscribe<HardwareFaultEvent>(this, Handle);
        }

        private void CheckLockUp()
        {
            //Checking if Hopper test lockup exist.
            CheckHopperTestLockup();

            var existingFaults = _properties.GetValue(HardwareConstants.HopperFaults, HopperFaultTypes.None);

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
            if (FaultTexts.ContainsKey(evt.Fault) &&
                !_hopperService.Faults.HasFlag(evt.Fault))
            {
                HandleLockUp(evt.Fault);
            }
        }

        private void HandleLockUp(HopperFaultTypes coinFault)
        {
            var descriptor = FaultTexts[coinFault];

            _hopperService.Faults |= coinFault;

            if (coinFault == HopperFaultTypes.IllegalCoinOut)
            {
                descriptor.LockUpMessage += $" {_tokenValue.MillicentsToDollars().FormattedCurrencyString()}";
            }

            _disableManager.Disable(
                coinFault.GetAttribute<ErrorGuidAttribute>().Id,
                SystemDisablePriority.Immediate,
                () => descriptor.LockUpMessage,
                true,
                () => descriptor.LockUpHelpMessage);

            _alarm.PlayAlarm();
            _properties.SetProperty(HardwareConstants.HopperFaults, _hopperService.Faults);
        }

        private void HardwareFaultClear()
        {
            if (_hopperService.Faults != HopperFaultTypes.None)
            {
                _hopperService.Faults = HopperFaultTypes.None;

                // Reset hopper.
                _hopperService.Reset();

                foreach (HopperFaultTypes fault in Enum.GetValues(typeof(HopperFaultTypes)))
                {
                    if (FaultTexts.ContainsKey(fault))
                    {
                        _disableManager.Enable(fault.GetAttribute<ErrorGuidAttribute>().Id);
                        _bus.Publish(new HardwareFaultClearEvent(fault));
                    }
                }

                _properties.SetProperty(HardwareConstants.HopperFaults, HopperFaultTypes.None);

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
        public void Dispose()
        {
            _bus.UnsubscribeAll(this);
        }
    }
}
