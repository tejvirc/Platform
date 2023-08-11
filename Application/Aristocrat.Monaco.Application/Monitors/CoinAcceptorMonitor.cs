﻿namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Hardware.Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.CoinAcceptor;
    using Kernel;
    using static Hardware.Contracts.CoinAcceptor.CoinEventsDescriptor;

    /// <summary>
    ///     Handle Lockup events from coin acceptor.
    /// </summary>
    public sealed class CoinAcceptorMonitor : IService, IDisposable
    {
        private readonly ISystemDisableManager _disableManager;
        private readonly IEventBus _bus;
        private readonly IPropertiesManager _properties;
        private readonly ICoinAcceptor _coinAcceptorService;
        private Alarm _alarm;

        public CoinAcceptorMonitor()
            : this(
                ServiceManager.GetInstance().TryGetService<ICoinAcceptor>(),
                ServiceManager.GetInstance().TryGetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        public CoinAcceptorMonitor(
            ICoinAcceptor coinAcceptorService,
            ISystemDisableManager disableManager,
            IPropertiesManager propertiesManager,
            IEventBus eventBus)
        {
            _coinAcceptorService = coinAcceptorService;
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public string Name => nameof(CoinAcceptorMonitor);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(CoinAcceptorMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            if(_coinAcceptorService == null)
            {
                return;
            }

            SubscribeEvents();

            _alarm = new Alarm();
            _alarm.LoadAlarm();

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
            var existingFaults = _properties.GetValue(HardwareConstants.CoinAcceptorFaults, CoinFaultTypes.None);

            foreach (CoinFaultTypes fault in Enum.GetValues(typeof(CoinFaultTypes)))
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
            if (_properties.GetValue(HardwareConstants.CoinAcceptorDiagnosticMode, false))
            {
                return;
            }

            if (FaultTexts.ContainsKey(evt.Fault) &&
                !_coinAcceptorService.Faults.HasFlag(evt.Fault))
            {
                HandleLockUp(evt.Fault);
            }
        }

        private void HandleLockUp(CoinFaultTypes coinFault)
        {
            var descriptor = FaultTexts[coinFault];

            _coinAcceptorService.Faults |= coinFault;

            _disableManager.Disable(
                coinFault.GetAttribute<ErrorGuidAttribute>().Id,
                SystemDisablePriority.Immediate,
                () => descriptor.LockUpMessage,
                true,
                () => descriptor.LockUpHelpMessage);
            _alarm.PlayAlarm();
            _properties.SetProperty(HardwareConstants.CoinAcceptorFaults, _coinAcceptorService.Faults);
        }

        private void HardwareFaultClear()
        {
            if (_coinAcceptorService.Faults != CoinFaultTypes.None)
            {
                _coinAcceptorService.Faults = CoinFaultTypes.None;

                // Reset coin acceptor first before disabling reject mechanism.
                _coinAcceptorService.Reset();

                foreach (CoinFaultTypes fault in Enum.GetValues(typeof(CoinFaultTypes)))
                {
                    if (FaultTexts.ContainsKey(fault))
                    {
                        _disableManager.Enable(fault.GetAttribute<ErrorGuidAttribute>().Id);
                        _bus.Publish(new HardwareFaultClearEvent(fault));
                    }

                }

                _properties.SetProperty(HardwareConstants.CoinAcceptorFaults, CoinFaultTypes.None);

            }
        }

        public void Dispose()
        {
            _bus.UnsubscribeAll(this);
        }
    }
}