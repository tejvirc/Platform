namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Timers;
    using Cabinet.Contracts;
    using Common;
    using Contracts.Cabinet;
    using Contracts.HardMeter;
    using Contracts.IO;
    using Contracts.SerialPorts;
    using Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using NativeOS.Services.IO;
    using Properties;
    using Constants = Kernel.Contracts.Components.Constants;
    using DisabledEvent = Contracts.IO.DisabledEvent;
    using EnabledEvent = Contracts.IO.EnabledEvent;
    using ErrorEventId = NativeOS.Services.IO.ErrorEventId;
    using Timer = System.Timers.Timer;

    /// <summary>
    ///     The IO Service component is the serviceable component which manages a physical IO implementation such as Innocore,
    ///     Tewes and
    ///     posts physical IO events to the system for logical interpretation.  The IO element is typically used by associated
    ///     logical IO services such as Button, Light.
    ///     This component does not provide an operator menu interface plug-in as this will be provides by the associated
    ///     logical
    ///     IO services.
    /// </summary>
    public class IOService : BaseRunnable, IDeviceService, IIO
    {
        private const int IOPollTimeMs = 100;
        private const ulong IntrusionMasks = 0X3F;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly Timer PollTimer = new();
        private static readonly AutoResetEvent Poll = new(true);

        private static readonly object QueuedEventsLock = new();

        private readonly IEventBus _eventBus;
        private readonly IComponentRegistry _componentRegistry;
        private readonly IIOProvider _inputOutput;
        private readonly ICabinetDetectionService _cabinetService;
        private readonly ISerialPortsService _serialPortsService;
        private readonly Collection<IEvent> _queuedEvents = new();
        private readonly IReadOnlyDictionary<int, Doors> _doorsMap;

        private bool _hardMeterStoppedResponding;

        private bool _pendingInspectedEvent;
        private bool _pendingInspectionFailedEvent;
        private bool _platformBooted;
        private bool _postedInspectionFailedEvent;

        public IOService(
            IEventBus eventBus,
            IComponentRegistry componentRegistry,
            ICabinetDetectionService cabinetService,
            ISerialPortsService serialPortsService,
            IIOProvider ioProvider)
        {
            _inputOutput = ioProvider ?? throw new ArgumentNullException(nameof(ioProvider));
            _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
            _serialPortsService = serialPortsService ?? throw new ArgumentNullException(nameof(serialPortsService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _inputOutput.ErrorOccurred += InputOutputOnErrorOccurred;

            Enabled = false;
            Initialized = false;
            _doorsMap = new Dictionary<int, Doors>
            {
                { 45, Doors.LogicDoor },
                { 48, Doors.DropDoor },
                { 50, Doors.CashDoor },
                { 49, Doors.MainDoor },
                { 51, Doors.BellyBoor }
            };
        }

        /// <summary>Gets or sets the last enabled logical state.</summary>
        /// <returns>Last enabled logical state.</returns>
        private static IOLogicalState LastEnabledLogicalState { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; private set; }

        /// <inheritdoc />
        public bool Initialized { get; private set; }

        /// <inheritdoc />
        public string LastError { get; private set; }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public DisabledReasons ReasonDisabled { get; private set; }

        /// <inheritdoc />
        public string ServiceProtocol { get; set; } = @"Gen8";

        /// <inheritdoc />
        [CLSCompliant(false)]
        public void Disable(DisabledReasons reason)
        {
            Logger.Debug($"{Name} disabled by {reason}");
            ReasonDisabled |= reason;
            Enabled = false;
            if (LogicalState != IOLogicalState.Disabled)
            {
                Logger.Debug($"Last enabled logical state {LastEnabledLogicalState} set to {LogicalState}");
                LastEnabledLogicalState = LogicalState;
            }

            LogicalState = IOLogicalState.Disabled;
            Logger.Debug("Logical state set to " + LogicalState);

            _eventBus.Publish(new DisabledEvent(ReasonDisabled));
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public bool Enable(EnabledReasons reason)
        {
            if (Enabled)
            {
                Logger.Debug(Name + " enabled by " + reason + " logical state " + LogicalState);
                _eventBus.Publish(new EnabledEvent(reason));
            }
            else if (Initialized)
            {
                RemoveDisabledReason(reason);

                Enabled = ReasonDisabled == 0;
                if (Enabled)
                {
                    Logger.Debug(Name + " enabled by " + reason + " logical state " + LogicalState);
                    if (LastEnabledLogicalState != IOLogicalState.Uninitialized &&
                        LastEnabledLogicalState != IOLogicalState.Idle)
                    {
                        Logger.Debug("Logical state " + LogicalState + " reset to " + IOLogicalState.Idle);
                        LogicalState = IOLogicalState.Idle;
                    }
                    else
                    {
                        Logger.Debug("Logical state " + LogicalState + " reset to " + LastEnabledLogicalState);
                        LogicalState = LastEnabledLogicalState;
                    }

                    _eventBus.Publish(new EnabledEvent(reason));
                }
                else
                {
                    Logger.Warn(Name + " can not be enabled by " + reason + " because disabled by " + ReasonDisabled);
                    _eventBus.Publish(new DisabledEvent(ReasonDisabled));
                }
            }
            else
            {
                Logger.Warn(Name + " can not be enabled by " + reason + " because service is not initialized");
                _eventBus.Publish(new DisabledEvent(ReasonDisabled));
            }

            return Enabled;
        }

        public string Name => nameof(IOService);

        public ICollection<Type> ServiceTypes => new[] { typeof(IIO) };

        /// <summary>Get whether the carrier board was removed during power-down or changed.</summary>
        /// <returns>true if board was removed, else false</returns>
        public bool WasCarrierBoardRemoved => _inputOutput.WasCarrierBoardRemoved;

        /// <inheritdoc />
        [CLSCompliant(false)]
        public Device DeviceConfiguration => new(_serialPortsService)
        {
            Manufacturer = "Aristocrat",
            Model = "MK7+",
            VariantName = _cabinetService.Type.ToString(),
            Protocol = HardwareFamilyIdentifier.Identify().ToString(),
            FirmwareId = _inputOutput.FirmwareId
        };

        /// <inheritdoc />
        public int GetMaxInputs => _inputOutput.AvailableInputs;

        /// <inheritdoc />
        public int GetMaxOutputs => _inputOutput.AvailableOutputs;

        /// <inheritdoc />
        [CLSCompliant(false)]
        public ulong GetInputs => _inputOutput.Inputs;

        /// <inheritdoc />
        [CLSCompliant(false)]
        public ulong GetOutputs => _inputOutput.Outputs;

        /// <inheritdoc />
        public ICollection<IEvent> GetQueuedEvents
        {
            get
            {
                lock (QueuedEventsLock)
                {
                    return new Collection<IEvent>(_queuedEvents);
                }
            }
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public ulong LastChangedInputs { get; set; }

        /// <inheritdoc />
        public IOLogicalState LogicalState { get; private set; }

        public void SetOutput(int physicalId, bool action, bool postActionEvent)
        {
            if (physicalId < 0 || physicalId >= _inputOutput.AvailableOutputs)
            {
                Logger.Error($"Invalid physical ID {physicalId}");
                return;
            }

            if (action)
            {
                _inputOutput.TurnOutputsOn((ulong)1 << physicalId);

                Logger.Debug($"Output {physicalId} on");

                if (postActionEvent)
                {
                    _eventBus.Publish(new OutputEvent(physicalId, true));
                }
            }
            else
            {
                _inputOutput.TurnOutputsOff((ulong)1 << physicalId);

                Logger.Debug($"Output {physicalId} off");

                if (postActionEvent)
                {
                    _eventBus.Publish(new OutputEvent(physicalId, false));
                }
            }
        }

        public void SetOutput32(int physicalId, bool action, bool postActionEvent)
        {
            ulong testBit = 1;

            if (action)
            {
                _inputOutput.TurnOutputsOn((ulong)physicalId);

                for (var i = 0; i < _inputOutput.AvailableInputs; i++)
                {
                    if (((ulong)physicalId & testBit) != 0)
                    {
                        Logger.Debug($"Turn Output {i} on");

                        if (postActionEvent)
                        {
                            _eventBus.Publish(new OutputEvent(i, true));
                        }
                    }

                    // Advance test bit.
                    testBit <<= 1;
                }
            }
            else
            {
                _inputOutput.TurnOutputsOff((ulong)physicalId);

                for (var i = 0; i < _inputOutput.AvailableInputs; i++)
                {
                    if (((ulong)physicalId & testBit) != 0)
                    {
                        Logger.Debug($"Turn Output {i} off");

                        if (postActionEvent)
                        {
                            _eventBus.Publish(new OutputEvent(i, false));
                        }
                    }

                    // Advance test bit.
                    testBit <<= 1;
                }
            }
        }

        public void SetMechanicalMeterLight(bool lightEnabled)
        {
            _inputOutput.SetMechanicalMeterLight(lightEnabled);
        }

        public int SetMechanicalMeter(int meterMask)
        {
            return _inputOutput.SetMechanicalMeter(meterMask);
        }

        public int ClearMechanicalMeter(int meterMask)
        {
            return _inputOutput.ClearMechanicalMeter(meterMask);
        }

        public int StatusMechanicalMeter(int meterMask)
        {
            return _inputOutput.StatusMechanicalMeter(meterMask);
        }

        [CLSCompliant(false)]
        public uint SetButtonLamp(uint lightIndex, bool lightStatusOn)
        {
            return _inputOutput.SetButtonLamp(lightIndex, lightStatusOn);
        }

        [CLSCompliant(false)]
        public uint SetButtonLampByMask(uint lightBits, bool lightStatusOn)
        {
            return _inputOutput.SetButtonLampByMask(lightBits, lightStatusOn);
        }

        public void ResetDoorSeal()
        {
            _inputOutput.ResetDoorSeal();
        }

        public byte GetDoorSealValue()
        {
            return _inputOutput.GetDoorSealValue();
        }

        public void SetLogicDoorSealValue(byte sealValue)
        {
            _inputOutput.SetLogicDoorSealValue(sealValue);
        }

        public byte GetLogicDoorSealValue()
        {
            return _inputOutput.GetLogicDoorSealValue();
        }

        public void ResetPhysicalDoorWasOpened(int physicalId)
        {
            if (!_doorsMap.TryGetValue(physicalId, out var door))
            {
                return;
            }

            _inputOutput.ResetPhysicalDoorWasOpened(door);
        }

        public bool GetPhysicalDoorWasOpened(int physicalId)
        {
            return _doorsMap.TryGetValue(physicalId, out var door) && _inputOutput.GetPhysicalDoorWasOpened(door);
        }

        public void SetKeyIndicator(int keyMask, bool lightEnabled)
        {
            _inputOutput.SetKeyIndicator(keyMask, lightEnabled);
        }

        public string GetElectronics()
        {
            return ServiceProtocol;
        }

        public byte[] GetFirmwareData(FirmwareData location)
        {
            return _inputOutput.GetFirmwareData((FirmwareType)location).ToArray();
        }

        public long GetFirmwareSize(FirmwareData location)
        {
            return (long)_inputOutput.GetFirmwareSize((FirmwareType)location);
        }

        public string GetFirmwareVersion(FirmwareData location)
        {
            return _inputOutput.GetFirmwareVersion((FirmwareType)location);
        }

        public bool TestBattery(int batteryIndex)
        {
            return _inputOutput.TestBatteryAsync((Battery)batteryIndex, CancellationToken.None).WaitForCompletion();
        }

        public bool SetBellState(bool ringBell)
        {
            return _inputOutput.SetBellState(ringBell);
        }

        public bool SetTowerLight(int lightIndex, bool lightOn)
        {
            return _inputOutput.SetTowerLight(lightIndex, lightOn);
        }

        public bool SetRedScreenFreeSpinBankShow(bool bankOn)
        {
            return _inputOutput.SetMultipurposeInputOutput(bankOn);
        }

        protected override void OnInitialize()
        {
            if (string.IsNullOrEmpty(ServiceProtocol))
            {
                var errorMessage = "Cannot initialize" + Name + ", must set device service protocol first";
                Logger.Error(errorMessage);
                throw new ServiceException(errorMessage);
            }

            // Set logical state uninitialized.
            LogicalState = IOLogicalState.Uninitialized;
            LastEnabledLogicalState = IOLogicalState.Uninitialized;

            // Reset pending event flags before we factory the implementation.
            _pendingInspectedEvent = false;
            _pendingInspectionFailedEvent = false;
            _postedInspectionFailedEvent = false;

            // Subscribe to all events before we factory the implementation.
            SubscribeToEvents();

            // Reset disabled reasons and last error.
            ReasonDisabled = 0;
            LastError = string.Empty;
            ServiceProtocol = HardwareFamilyIdentifier.Identify().ToString();

            // Load an instance of the given protocol implementation.
            if (_inputOutput is null)
            {
                var errorMessage = "Cannot load" + Name;
                Logger.Error(errorMessage);
                throw new ServiceException(errorMessage);
            }

            Logger.Debug($"Created IOImplementation: {ServiceProtocol}");

            // Initialize the device implementation.
            _inputOutput.InitializeAsync(CancellationToken.None).WaitForCompletion();
            _pendingInspectedEvent = true;

            // Initialize last changed inputs (all off).
            LastChangedInputs = IntrusionMasks << 32;

            // This takes care of initializing the Base Door and Alt Doors 1 - 9.
            // If physical door configurations are ever modified.  This will need to be modified as well.
            LastChangedInputs |= 0x00ff0000;

            // Get array of InputEvent's for door opens and closes that occurred while machine was powered down
            // Iterate through events, add to queue, and set/clear bits in _lastChangedInputs
            // Note: Bits for doors are in the upper 32 bits of _lastChangedInputs. Physical ID in the event corresponds
            // to bit number when counting from right to left starting with 0.
            var intrusionEvents = _inputOutput.IntrusionEvents;
            foreach (var intrusion in intrusionEvents)
            {
                var doorId = _doorsMap.FirstOrDefault(d => d.Value == intrusion.Id).Key;
                lock (QueuedEventsLock)
                {
                    Logger.Debug(
                        $"Adding intrusion event for door {intrusion.Id} {intrusion.Action} to event queue. size is {_queuedEvents.Count}");
                    _queuedEvents.Add(new InputEvent(doorId, intrusion.Action));
                }

                ulong setOrClearBit = 1;
                setOrClearBit <<= doorId;

                // Action is true for door closed; false for door open
                if (intrusion.Action)
                {
                    LastChangedInputs |= setOrClearBit;
                }
                else
                {
                    LastChangedInputs &= ~setOrClearBit;
                }
            }

            if (_inputOutput.WasCarrierBoardRemoved)
            {
                Logger.Debug("Carrier board removal/change detected.");
            }

            // Set poll timer to elapsed event.
            PollTimer.Elapsed += OnPollTimeout;

            // Set poll timer interval to implementation polling frequency and start.
            PollTimer.Interval = IOPollTimeMs;
            PollTimer.Start();

            // Set service initialized.
            Initialized = true;

            Logger.Debug($"{Name} initialized");

            // Register the firmware components.
            RegisterHardwareDevice(
                ComponentType.Hardware,
                Constants.BiosPath,
                $"BIOS-{GetFirmwareVersion(FirmwareData.Bios)}",
                Resources.BIOSPackageDescription,
                GetFirmwareSize(FirmwareData.Bios));
        }

        protected override void OnRun()
        {
            if (!CanProceed(false))
            {
                return;
            }

            Logger.Debug($"{Name} started");

            while (RunState == RunnableState.Running)
            {
                // Do we have a pending inspected event?
                if (_pendingInspectedEvent)
                {
                    _pendingInspectedEvent = false;

                    // Disable the service for configuration.  This service should only be enabled by configuration after
                    // all logical IO services have been started so that all physical input events are handled.
                    Disable(DisabledReasons.Configuration);

                    // Set last logical state to current disabled logical state so we transition to idle when enabled.
                    LastEnabledLogicalState = LogicalState;
                }
                else if (_pendingInspectionFailedEvent)
                {
                    _postedInspectionFailedEvent = true;
                    _pendingInspectionFailedEvent = false;

                    // Disable service for error.
                    Disable(DisabledReasons.Error);
                }

                // Block the thread until it is time to poll.
                Poll.WaitOne();

                if (RunState != RunnableState.Running ||
                    LogicalState != IOLogicalState.Idle &&
                    (LogicalState != IOLogicalState.Disabled || !_hardMeterStoppedResponding))
                {
                    continue;
                }

                var inputs = _inputOutput.Inputs;
                if (inputs == LastChangedInputs)
                {
                    continue;
                }

                Logger.DebugFormat("Inputs {0}", FormatBits(_inputOutput.AvailableInputs, 4, inputs));

                // Post an event for each changed input.
                ulong testBit = 1;
                for (var i = 0; i < _inputOutput.AvailableInputs; i++)
                {
                    int physicalId;
                    if ((inputs & testBit) != 0 && (LastChangedInputs & testBit) == 0)
                    {
                        physicalId = i;
                        Logger.Debug($"Queuing Input {physicalId} on. size is {_queuedEvents.Count}");
                        var inputEvent = new InputEvent(physicalId, true);
                        if (!_platformBooted)
                        {
                            lock (QueuedEventsLock)
                            {
                                _queuedEvents.Add(inputEvent);
                            }
                        }

                        _eventBus.Publish(inputEvent);
                    }
                    else if ((inputs & testBit) == 0 && (LastChangedInputs & testBit) != 0)
                    {
                        physicalId = i;
                        Logger.Debug($"Queuing Input {physicalId} off. size is {_queuedEvents.Count}");
                        var inputEvent = new InputEvent(physicalId, false);
                        if (!_platformBooted)
                        {
                            lock (QueuedEventsLock)
                            {
                                _queuedEvents.Add(inputEvent);
                            }
                        }

                        _eventBus.Publish(inputEvent);
                    }

                    testBit <<= 1;
                }

                // Set last changed inputs.
                LastChangedInputs = inputs;
            }

            // Unsubscribe from all events.
            UnsubscribeFromEvents();

            // Clean up the device implementation.
            _inputOutput.Dispose();
            Logger.Debug($"{Name} stopped");
        }

        protected override void OnStop()
        {
            Logger.Debug($"{Name} stopping");

            // Disable the logical service.
            Disable(DisabledReasons.Service);

            // Set poll event to unblock the runnable in order to stop.
            Poll.Set();
        }

        private static string FormatBits(int max, int spacer, ulong value)
        {
            var formattedBits = new StringBuilder();
            for (var i = 0; i < max; i++)
            {
                if (spacer > 0 && i % spacer == 0)
                {
                    formattedBits.Append(' ');
                }

                formattedBits.Append((value & (1UL << i)) != 0 ? '1' : '0');
            }

            return formattedBits.ToString();
        }

        private static void OnPollTimeout(object sender, ElapsedEventArgs e)
        {
            // Set to poll.
            Poll.Set();
        }

        private void RemoveDisabledReason(EnabledReasons reason)
        {
            if (((ReasonDisabled & DisabledReasons.Error) > 0 ||
                 (ReasonDisabled & DisabledReasons.FirmwareUpdate) > 0) &&
                (reason == EnabledReasons.Reset || reason == EnabledReasons.Operator))
            {
                if ((ReasonDisabled & DisabledReasons.Error) > 0)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.Error);
                    ReasonDisabled &= ~DisabledReasons.Error;
                }

                if ((ReasonDisabled & DisabledReasons.FirmwareUpdate) > 0)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.FirmwareUpdate);
                    ReasonDisabled &= ~DisabledReasons.FirmwareUpdate;
                }
            }
            else if ((ReasonDisabled & DisabledReasons.Operator) > 0 && reason == EnabledReasons.Operator)
            {
                Logger.Debug("Removed disabled reason " + DisabledReasons.Operator);
                ReasonDisabled &= ~DisabledReasons.Operator;
            }
            else if ((ReasonDisabled & DisabledReasons.Service) > 0 && reason == EnabledReasons.Service)
            {
                Logger.Debug("Removed disabled reason " + DisabledReasons.Service);
                ReasonDisabled &= ~DisabledReasons.Service;
            }
            else if ((ReasonDisabled & DisabledReasons.System) > 0 && reason == EnabledReasons.System)
            {
                Logger.Debug("Removed disabled reason " + DisabledReasons.System);
                ReasonDisabled &= ~DisabledReasons.System;
            }
            else if ((ReasonDisabled & DisabledReasons.Configuration) > 0 && reason == EnabledReasons.Configuration)
            {
                Logger.Debug("Removed disabled reason " + DisabledReasons.Configuration);
                ReasonDisabled &= ~DisabledReasons.Configuration;
            }
        }

        private bool CanProceed(bool checkEnabled)
        {
            if (!Initialized)
            {
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(1);
                var methodBase = stackFrame.GetMethod();
                Logger.ErrorFormat("{0} cannot proceed, must initialize {1} first", methodBase.Name, Name);
                return false;
            }

            if (checkEnabled && !Enabled)
            {
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(1);
                var methodBase = stackFrame.GetMethod();
                Logger.ErrorFormat("{0} cannot proceed, must enable {1} first", methodBase.Name, Name);
                _eventBus.Publish(new DisabledEvent(ReasonDisabled));
                return false;
            }

            return true;
        }

        private void SubscribeToEvents()
        {
            // Subscribe to initialization events.
            _eventBus.Subscribe<PlatformBootedEvent>(this, ReceiveEvent);
            _eventBus.Subscribe<StoppedRespondingEvent>(
                this,
                _ =>
                {
                    _hardMeterStoppedResponding = true;
                    Disable(DisabledReasons.Error);
                });
            _eventBus.Subscribe<StartedRespondingEvent>(
                this,
                _ =>
                {
                    _hardMeterStoppedResponding = false;
                    Enable(EnabledReasons.Reset);
                });
        }

        private void UnsubscribeFromEvents()
        {
            // Unsubscribe from initialization events.
            _eventBus.UnsubscribeAll(this);
        }

        private void InputOutputOnErrorOccurred(object sender, HardwareErrorEventArgs e)
        {
            switch (e.Error)
            {
                case ErrorEventId.None:
                case ErrorEventId.Read:
                case ErrorEventId.Write:
                case ErrorEventId.WatchdogDisableFailure:
                case ErrorEventId.WatchdogResetFailure:
                    Logger.Info($"Handled ErrorEventId {e.Error}");
                    break;
                case ErrorEventId.InputFailure:
                case ErrorEventId.OutputFailure:
                case ErrorEventId.InvalidHandle:
                case ErrorEventId.ReadBoardInfoFailure:
                    Logger.Info($"Handled ErrorEventId {e.Error}");

                    if (LogicalState == IOLogicalState.Uninitialized && !_postedInspectionFailedEvent)
                    {
                        _pendingInspectionFailedEvent = true;
                    }
                    else
                    {
                        _hardMeterStoppedResponding = false;
                        Disable(DisabledReasons.Error);
                    }

                    break;
                default:
                    Logger.Info($"Unhandled ErrorEventId {e.Error}");
                    break;
            }
        }

        private void ReceiveEvent(PlatformBootedEvent data)
        {
            _platformBooted = true;
        }

        private void RegisterHardwareDevice(
            ComponentType componentType,
            string path,
            string componentId,
            string description,
            long size)
        {
            if (size <= 0)
            {
                return;
            }

            var component = new Component
            {
                ComponentId = $"ATI_{componentId}".Replace(" ", "_"),
                Description = description,
                Path = path,
                Size = size,
                Type = componentType,
                FileSystemType = FileSystemType.Stream
            };

            _componentRegistry.Register(component);
        }
    }
}