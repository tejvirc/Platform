namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Dfu;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.MarketConfig;
    using Kernel.MarketConfig.Models.Application;
    using Util;
    using DisabledEvent = Hardware.Contracts.Printer.DisabledEvent;
    using EnabledEvent = Hardware.Contracts.Printer.EnabledEvent;

    /// <summary>
    ///     Definition of the PrinterMonitor class.
    /// </summary>
    public class PrinterMonitor : GenericBaseMonitor, IPrinterMonitor, IService
    {
        private const string BlockName = "Aristocrat.Monaco.Application.Monitors.PrinterMonitor";
        private const string PrintingKey = "Printing";
        private const string LastFaultFlagsKey = "LastFaultFlags";

        private readonly IAudio _audioService = ServiceManager.GetInstance().GetService<IAudio>();
        private readonly IEventBus _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
        private readonly IMeterManager _meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();
        private readonly IPersistentStorageManager _persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
        private readonly IPropertiesManager _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

        private string _printerErrorSoundFilePath;
        private string _printerWarningSoundFilePath;
        private bool _stopAlarmWhenAuditMenuOpened;

        /// <inheritdoc />
        public override string DeviceName => "Printer";

        /// <inheritdoc />
        public string Name => nameof(PrinterMonitor);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPrinterMonitor) };

        private bool _inOperatorMode;

        /// <inheritdoc />
        public void Initialize()
        {
            if (!_persistentStorage.BlockExists(BlockName))
            {
                var block = _persistentStorage.CreateBlock(PersistenceLevel.Transient, BlockName, 1);
                using (var transaction = block.StartTransaction())
                {
                    transaction[DisconnectedKey] = false;
                    transaction[LastFaultFlagsKey] = PrinterFaultTypes.None;
                    transaction.Commit();
                }
            }

            Configure();

            // These are the sets of errors and states that this class monitors uniquely.
            ManageErrorEnum<PrinterFaultTypes>(
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                true);
            ManageErrorEnum<PrinterWarningTypes>(DisplayableMessagePriority.Normal);
            ManageBinaryCondition(
                PrintingKey,
                DisplayableMessageClassification.Diagnostic,
                DisplayableMessagePriority.Normal,
                new Guid("{80E64F46-164E-4C5A-9DC6-0BDC3474E4D5}"));
            ManageBinaryCondition(
                DisconnectedKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                ApplicationConstants.PrinterDisconnectedGuid,
                true);
            ManageBinaryCondition(
                DisabledKey,
                DisplayableMessageClassification.SoftError,
                DisplayableMessagePriority.Immediate,
                new Guid("{551A42FF-C912-44F2-8AFC-D953A91FC308}"));
            ManageBinaryCondition(
                DfuInprogressKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                new Guid("{ACE044E3-D342-44FC-B7FC-E29B624FD821}"),
                true);

            ServiceManager.GetInstance().GetService<IPropertiesManager>().AddPropertyProvider(this);

            SubscribeToEvents();

            LoadSounds();

            CheckPrinterStatus();
        }

        private void Configure()
        {
            var marketConfigManager = ServiceManager.GetInstance().GetService<IMarketConfigManager>();

            var configuration = marketConfigManager.GetMarketConfigForSelectedJurisdiction<ApplicationConfigSegment>();

            _stopAlarmWhenAuditMenuOpened = configuration.PrinterMonitor.StopAlarmWhenAuditMenuOpened;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<PrintCompletedEvent>(this, _ => { SetBinary(PrintingKey, false); });
            _eventBus.Subscribe<PrintStartedEvent>(this, _ => { SetBinary(PrintingKey, true); });
            _eventBus.Subscribe<ConnectedEvent>(this, _ => { Disconnected(false); });
            _eventBus.Subscribe<DisconnectedEvent>(this,
                _ =>
                {
                    Disconnected(true);
                    PlayPrinterErrorSound();
                });
            _eventBus.Subscribe<EnabledEvent>(this, _ => { SetBinary(DisabledKey, false); });
            _eventBus.Subscribe<DisabledEvent>(this, _ => { SetBinary(DisabledKey, true); });
            _eventBus.Subscribe<HardwareWarningClearEvent>(this, Handle);
            _eventBus.Subscribe<HardwareWarningEvent>(this, Handle);
            _eventBus.Subscribe<HardwareFaultClearEvent>(this, Handle);
            _eventBus.Subscribe<HardwareFaultEvent>(this, Handle);
            _eventBus.Subscribe<InspectionFailedEvent>(this, _ => { Disconnected(true); });
            _eventBus.Subscribe<InspectedEvent>(this, _ => { Disconnected(false); });
            _eventBus.Subscribe<DfuDownloadStartEvent>(this,
                e =>
                {
                    if (e.Device == DeviceType.Printer)
                    {
                        SetBinary(DfuInprogressKey, true);
                    }
                });
            _eventBus.Subscribe<DfuErrorEvent>(this, _ => { SetBinary(DfuInprogressKey, false); });
            _eventBus.Subscribe<FirmwareInstalledEvent>(this, _ => { SetBinary(DfuInprogressKey, false); });
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, Handle);
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, Handle);
        }

        private void Disconnected(bool disconnected)
        {
            using (var scope = _persistentStorage.ScopedTransaction())
            {
                var block = _persistentStorage.GetBlock(BlockName);

                if (disconnected && !(bool)block[DisconnectedKey])
                {
                    _meterManager.GetMeter(ApplicationMeters.PrinterDisconnectCount).Increment(1);
                }

                using (var transaction = block.StartTransaction())
                {
                    transaction[DisconnectedKey] = disconnected;
                    transaction.Commit();
                }

                scope.Complete();
            }

            SetBinary(DisconnectedKey, disconnected);
        }

        private void CheckPrinterStatus()
        {
            var device = ServiceManager.GetInstance().TryGetService<IPrinter>();
            if (device == null)
            {
                return;
            }

            if (!device.Enabled || device.Faults != PrinterFaultTypes.None)
            {
                if (device.Faults != PrinterFaultTypes.None)
                {
                    Handle(new HardwareFaultEvent(device.Faults));
                }
                else
                {
                    SetBinary(DisabledKey, true);
                }
            }

            if (device.Warnings != PrinterWarningTypes.None)
            {
                Handle(new HardwareWarningEvent(device.Warnings));
            }

            if (device.LogicalState == PrinterLogicalState.Uninitialized)
            {
                Disconnected(true);
            }
        }

        private void LoadSounds()
        {
            _printerErrorSoundFilePath = _propertiesManager?.GetValue(ApplicationConstants.PrinterErrorSoundKey, string.Empty);
            _audioService.LoadSound(_printerErrorSoundFilePath);

            _printerWarningSoundFilePath = _propertiesManager?.GetValue(
                ApplicationConstants.PrinterWarningSoundKey,
                string.Empty);
            _audioService.LoadSound(_printerWarningSoundFilePath);
        }

        /// <summary>
        /// Plays the sound defined in the Application Config for PaperEmptySound.
        /// </summary>
        private void PlayPrinterErrorSound()
        {
            if (!_inOperatorMode)
            {
                _audioService.PlaySound(_propertiesManager, _printerErrorSoundFilePath);
            }
        }

        /// <summary>
        /// Plays the sound defined in the Application Config for PaperLow condition.
        /// </summary>
        private void PlayPrinterWarningSound()
        {
            if (!_inOperatorMode)
            {
                _audioService.PlaySound(_propertiesManager, _printerWarningSoundFilePath);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed state (managed objects).
                _eventBus.UnsubscribeAll(this);
            }
        }

        private void Handle(HardwareWarningClearEvent obj)
        {
            var faults = obj.Warning;

            foreach (PrinterWarningTypes e in Enum.GetValues(typeof(PrinterWarningTypes)))
            {
                if (e != PrinterWarningTypes.None && faults.HasFlag(e))
                {
                    ClearFault(e);
                }
            }
        }

        private void Handle(HardwareWarningEvent obj)
        {
            var faults = obj.Warning;

            foreach (PrinterWarningTypes e in Enum.GetValues(typeof(PrinterWarningTypes)))
            {
                if (e != PrinterWarningTypes.None && faults.HasFlag(e))
                {
                    AddFault(e);

                    // PaperInChute has different warning sound handled in CashoutController
                    if (e != PrinterWarningTypes.PaperInChute)
                    {
                        PlayPrinterWarningSound();
                    }
                }
            }
        }

        private void Handle(HardwareFaultClearEvent obj)
        {
            var faults = obj.Fault;

            var block = _persistentStorage.GetBlock(BlockName);
            var lastFaults = (PrinterFaultTypes)block[LastFaultFlagsKey];

            foreach (PrinterFaultTypes e in Enum.GetValues(typeof(PrinterFaultTypes)))
            {
                if (e != PrinterFaultTypes.None && faults.HasFlag(e))
                {
                    ClearFault(e);
                    lastFaults &= ~e;
                }
            }

            // Store the last known fault status
            using (var transaction = block.StartTransaction())
            {
                transaction[LastFaultFlagsKey] = lastFaults;
                transaction.Commit();
            }
        }

        private void Handle(HardwareFaultEvent obj)
        {
            var faults = obj.Fault;

            var block = _persistentStorage.GetBlock(BlockName);
            var lastFaults = (PrinterFaultTypes)block[LastFaultFlagsKey];

            foreach (PrinterFaultTypes e in Enum.GetValues(typeof(PrinterFaultTypes)))
            {
                if (e != PrinterFaultTypes.None && faults.HasFlag(e))
                {
                    AddFault(e);

                    if (e != PrinterFaultTypes.PaperNotTopOfForm)
                    {
                        if (!lastFaults.HasFlag(e))
                        {
                            _meterManager.GetMeter(ApplicationMeters.PrinterErrorCount).Increment(1);
                        }

                        PlayPrinterErrorSound();
                    }

                    lastFaults |= e;
                }
            }

            // Store the last known fault status
            using (var transaction = block.StartTransaction())
            {
                transaction[LastFaultFlagsKey] = lastFaults;
                transaction.Commit();
            }
        }

        private void Handle(OperatorMenuEnteredEvent obj)
        {
            if (_stopAlarmWhenAuditMenuOpened)
            {
                _inOperatorMode = true;
            }
        }

        private void Handle(OperatorMenuExitedEvent obj)
        {
            _inOperatorMode = false;
        }
    }
}