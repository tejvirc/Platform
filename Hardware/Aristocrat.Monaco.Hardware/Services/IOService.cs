namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Timers;
    using Cabinet.Contracts;
    using Contracts.HardMeter;
    using Contracts.IO;
    using Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using Properties;
    using DisabledEvent = Contracts.IO.DisabledEvent;
    using EnabledEvent = Contracts.IO.EnabledEvent;
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
        private const string DeviceImplementationsExtensionPath = "/Hardware/IO/IOImplementations";
        private const ulong IntrusionMasks = 0X3F;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Timer PollTimer = new Timer();
        private static readonly AutoResetEvent Poll = new AutoResetEvent(true);

        private static readonly object QueuedEventsLock = new object();

        private readonly DeviceAddinHelper _addinHelper = new DeviceAddinHelper();
        private readonly Collection<IEvent> _queuedEvents = new Collection<IEvent>();

        private bool _hardMeterStoppedResponding;
        private IIOImplementation _inputOutput;

        private bool _pendingInspectedEvent;
        private bool _pendingInspectionFailedEvent;
        private bool _platformBooted;
        private bool _postedInspectionFailedEvent;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IOService" /> class.
        /// </summary>
        public IOService()
        {
            Enabled = false;
            Initialized = false;
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

        /// <summary>Get whether the carrier board was removed during power-down or changed.</summary>
        /// <returns>true if board was removed, else false</returns>
        public bool WasCarrierBoardRemoved => _inputOutput.WasCarrierBoardRemoved;

        /// <inheritdoc />
        [CLSCompliant(false)]
        public void Disable(DisabledReasons reason)
        {
            var eventBus = ServiceManager.GetInstance().TryGetService<IEventBus>();
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

            eventBus?.Publish(new DisabledEvent(ReasonDisabled));
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public bool Enable(EnabledReasons reason)
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            if (Enabled)
            {
                Logger.Debug(Name + " enabled by " + reason + " logical state " + LogicalState);
                eventBus.Publish(new EnabledEvent(reason));
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

                    eventBus.Publish(new EnabledEvent(reason));
                }
                else
                {
                    Logger.Warn(Name + " can not be enabled by " + reason + " because disabled by " + ReasonDisabled);
                    eventBus.Publish(new DisabledEvent(ReasonDisabled));
                }
            }
            else
            {
                Logger.Warn(Name + " can not be enabled by " + reason + " because service is not initialized");
                eventBus.Publish(new DisabledEvent(ReasonDisabled));
            }

            return Enabled;
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public Device DeviceConfiguration => _inputOutput.DeviceConfiguration;

        /// <inheritdoc />
        public int GetMaxInputs => _inputOutput.GetMaxInputs;

        /// <inheritdoc />
        public int GetMaxOutputs => _inputOutput.GetMaxOutputs;

        /// <inheritdoc />
        [CLSCompliant(false)]
        public ulong GetInputs => _inputOutput.GetInputs;

        /// <inheritdoc />
        [CLSCompliant(false)]
        public ulong GetOutputs => _inputOutput.GetOutputs;

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
        public int GetWatchdogEnabled => _inputOutput.GetWatchdogEnabled;

        /// <inheritdoc />
        [CLSCompliant(false)]
        public ulong LastChangedInputs { get; set; }

        /// <inheritdoc />
        public IOLogicalState LogicalState { get; private set; }

        /// <inheritdoc />
        public int EnableWatchdog(int seconds)
        {
            return _inputOutput.EnableWatchdog(seconds);
        }

        public int ResetWatchdog()
        {
            return _inputOutput.ResetWatchdog();
        }

        public void SetOutput(int physicalId, bool action, bool postActionEvent)
        {
            if (physicalId < 0 || physicalId >= _inputOutput.GetMaxOutputs)
            {
                Logger.ErrorFormat($"Invalid physical ID {physicalId}");
                return;
            }

            if (action)
            {
                _inputOutput.TurnOutputsOn((ulong)1 << physicalId);

                Logger.Debug($"Output {physicalId} on");

                if (postActionEvent)
                {
                    var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                    eventBus.Publish(new OutputEvent(physicalId, true));
                }
            }
            else
            {
                _inputOutput.TurnOutputsOff((ulong)1 << physicalId);

                Logger.Debug($"Output {physicalId} off");

                if (postActionEvent)
                {
                    var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                    eventBus.Publish(new OutputEvent(physicalId, false));
                }
            }
        }

        public void SetOutput32(int physicalId, bool action, bool postActionEvent)
        {
            ulong testBit = 1;

            if (action)
            {
                _inputOutput.TurnOutputsOn((ulong)physicalId);

                for (var i = 0; i < _inputOutput.GetMaxInputs; i++)
                {
                    if (((ulong)physicalId & testBit) != 0)
                    {
                        Logger.Debug($"Turn Output {i} on");

                        if (postActionEvent)
                        {
                            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                            eventBus.Publish(new OutputEvent(i, true));
                        }
                    }

                    // Advance test bit.
                    testBit = testBit << 1;
                }
            }
            else
            {
                _inputOutput.TurnOutputsOff((ulong)physicalId);

                for (var i = 0; i < _inputOutput.GetMaxInputs; i++)
                {
                    if (((ulong)physicalId & testBit) != 0)
                    {
                        Logger.Debug($"Turn Output {i} off");

                        if (postActionEvent)
                        {
                            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                            eventBus.Publish(new OutputEvent(i, false));
                        }
                    }

                    // Advance test bit.
                    testBit = testBit << 1;
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
            _inputOutput.ResetPhysicalDoorWasOpened(physicalId);
        }

        public bool GetPhysicalDoorWasOpened(int physicalId)
        {
            return _inputOutput.GetPhysicalDoorWasOpened(physicalId);
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
            return _inputOutput.GetFirmwareData(location);
        }

        public long GetFirmwareSize(FirmwareData location)
        {
            return (long)_inputOutput.GetFirmwareSize(location);
        }

        public string GetFirmwareVersion(FirmwareData location)
        {
            return _inputOutput.GetFirmwareVersion(location);
        }

        public bool TestBattery(int batteryIndex)
        {
            return _inputOutput.TestBattery(batteryIndex);
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
            return _inputOutput.SetRedScreenFreeSpinBankShow(bankOn);
        }

        public string Name => nameof(IOService);

        public ICollection<Type> ServiceTypes => new[] { typeof(IIO) };

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
            _inputOutput = (IIOImplementation)_addinHelper.GetDeviceImplementationObject(
                DeviceImplementationsExtensionPath,
                ServiceProtocol);
            if (_inputOutput == null)
            {
                var errorMessage = "Cannot load" + Name;
                Logger.Error(errorMessage);
                throw new ServiceException(errorMessage);
            }

            Logger.Debug($"Created IOImplementation: {ServiceProtocol}");

            // Initialize the device implementation.
            _inputOutput.Initialize();

            // Initialize last changed inputs (all off).
            LastChangedInputs = IntrusionMasks << 32;

            // This takes care of initializing the Base Door and Alt Doors 1 - 9.
            // If physical door configurations are ever modified.  This will need to be modified as well.
            LastChangedInputs |= 0x00ff0000;

            // Get array of InputEvent's for door opens and closes that occurred while machine was powered down
            // Iterate through events, add to queue, and set/clear bits in _lastChangedInputs
            // Note: Bits for doors are in the upper 32 bits of _lastChangedInputs. Physical ID in the event corresponds
            // to bit number when counting from right to left starting with 0.
            var intrusionEvents = _inputOutput.GetIntrusionEvents;
            foreach (var intrusion in intrusionEvents)
            {
                lock (QueuedEventsLock)
                {
                    Logger.Debug(
                        $"Adding intrusion event for door {intrusion.Id} {intrusion.Action} to event queue. size is {_queuedEvents.Count}");
                    _queuedEvents.Add(intrusion);
                }

                ulong setOrClearBit = 1;
                setOrClearBit = setOrClearBit << intrusion.Id;

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
            var device = _inputOutput.DeviceConfiguration;
            PollTimer.Interval = device.PollingFrequency;
            PollTimer.Start();

            // Set service initialized.
            Initialized = true;

            Logger.Debug(Name + " initialized");

            // Register the firmware components.
            RegisterHardwareDevice(
                ComponentType.Hardware,
                Constants.BiosPath,
                $"BIOS-{GetFirmwareVersion(FirmwareData.Bios)}",
                Resources.BIOSPackageDescription,
                GetFirmwareSize(FirmwareData.Bios));

            //TODO:  We have to support a chunked based read of the FPGA data due to the size and blocking while reading
            /*
            RegisterHardwareDevice(
                ComponentType.Hardware,
                Constants.FpgaPath,
                $"FPGA-{GetFirmwareVersion(FirmwareData.Fpga)}",
                Resources.FPGAPackageDescription,
                GetFirmwareSize(FirmwareData.Fpga));
                */
        }

        protected override void OnRun()
        {
            if (CanProceed(false) == false)
            {
                return;
            }

            Logger.Debug(Name + " started");

            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

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

                if (RunState == RunnableState.Running)
                {
                    if (LogicalState == IOLogicalState.Idle ||
                        LogicalState == IOLogicalState.Disabled && _hardMeterStoppedResponding)
                    {
                        var inputs = _inputOutput.GetInputs;

                        if (inputs != LastChangedInputs)
                        {
                            Logger.DebugFormat("Inputs {0}", FormatBits(_inputOutput.GetMaxInputs, 4, inputs));

                            // Post an event for each changed input.
                            ulong testBit = 1;
                            for (var i = 0; i < _inputOutput.GetMaxInputs; i++)
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

                                    eventBus.Publish(inputEvent);
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

                                    eventBus.Publish(inputEvent);
                                }

                                testBit = testBit << 1;
                            }

                            // Set last changed inputs.
                            LastChangedInputs = inputs;
                        }
                    }
                }
            }

            // Unsubscribe from all events.
            UnsubscribeFromEvents();

            // Clean up the device implementation.
            _inputOutput.Cleanup();

            Logger.Debug(Name + " stopped");
        }

        protected override void OnStop()
        {
            Logger.Debug(Name + " stopping");

            // Disable the logical service.
            Disable(DisabledReasons.Service);

            // Set poll event to unblock the runnable in order to stop.
            Poll.Set();
        }

        private static string FormatBits(int max, int spacer, ulong value)
        {
            var formattedBits = string.Empty;
            for (var i = 0; i < max; i++)
            {
                if (spacer > 0)
                {
                    if (i % spacer == 0)
                    {
                        formattedBits += " ";
                    }
                }

                formattedBits += (value & (1UL << i)) != 0 ? "1" : "0";
            }

            return formattedBits;
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
            if (Initialized == false)
            {
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(1);
                var methodBase = stackFrame.GetMethod();
                Logger.ErrorFormat("{0} cannot proceed, must initialize {1} first", methodBase.Name, Name);
                return false;
            }

            if (checkEnabled && Enabled == false)
            {
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(1);
                var methodBase = stackFrame.GetMethod();
                Logger.ErrorFormat("{0} cannot proceed, must enable {1} first", methodBase.Name, Name);
                var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                eventBus.Publish(new DisabledEvent(ReasonDisabled));
                return false;
            }

            return true;
        }

        private void SubscribeToEvents()
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            // Subscribe to initialization events.
            eventBus.Subscribe<InitCompleteEvent>(this, ReceiveEvent);
            eventBus.Subscribe<ErrorEvent>(this, ReceiveEvent);
            eventBus.Subscribe<PlatformBootedEvent>(this, ReceiveEvent);
            eventBus.Subscribe<StoppedRespondingEvent>(
                this,
                _ =>
                {
                    _hardMeterStoppedResponding = true;
                    Disable(DisabledReasons.Error);
                });
            eventBus.Subscribe<StartedRespondingEvent>(
                this,
                _ =>
                {
                    _hardMeterStoppedResponding = false;
                    Enable(EnabledReasons.Reset);
                });
        }

        private void UnsubscribeFromEvents()
        {
            var eventBus = ServiceManager.GetInstance().TryGetService<IEventBus>();

            // Unsubscribe from initialization events.
            eventBus?.UnsubscribeAll(this);
        }

        private void ReceiveEvent(IEvent data)
        {
            if (typeof(ErrorEvent) == data.GetType())
            {
                var errorEvent = (ErrorEvent)data;
                var id = errorEvent.Id;

                LastError = id.ToString();

                switch (id)
                {
                    case ErrorEventId.InputFailure:
                    case ErrorEventId.OutputFailure:
                    case ErrorEventId.InvalidHandle:
                    case ErrorEventId.ReadBoardInfoFailure:
                    {
                        Logger.ErrorFormat("Handled ErrorEventId {0}", id);

                        // Are we uninitialized?
                        if (LogicalState == IOLogicalState.Uninitialized && _postedInspectionFailedEvent == false)
                        {
                            // Yes, set pending inspection failed event.
                            _pendingInspectionFailedEvent = true;
                        }
                        else
                        {
                            _hardMeterStoppedResponding = false;
                            // Disable service for error.
                            Disable(DisabledReasons.Error);
                        }

                        break;
                    }

                    case ErrorEventId.Read:
                    case ErrorEventId.Write:
                    case ErrorEventId.WatchdogEnableFailure:
                    case ErrorEventId.WatchdogDisableFailure:
                    case ErrorEventId.WatchdogResetFailure:
                    {
                        Logger.InfoFormat("Handled ErrorEventId {0}", id);
                        break;
                    }

                    default:
                        Logger.InfoFormat("Unhandled ErrorEventId {0}", id);
                        break;
                }
            }
            else if (typeof(InitCompleteEvent) == data.GetType())
            {
                // Set pending inspected event.
                _pendingInspectedEvent = true;
            }
            else if (typeof(PlatformBootedEvent) == data.GetType())
            {
                _platformBooted = true;
            }
            else
            {
                Logger.InfoFormat("Received unexpected event of type {0}", data.GetType());
            }
        }

        private void RegisterHardwareDevice(
            ComponentType componentType,
            string path,
            string componentId,
            string description,
            long size)
        {
            if (size > 0)
            {
                var component = new Component
                {
                    ComponentId = ("ATI_" + componentId).Replace(" ", "_"),
                    Description = description,
                    Path = path,
                    Size = size,
                    Type = componentType,
                    FileSystemType = FileSystemType.Stream
                };

                ServiceManager.GetInstance().GetService<IComponentRegistry>().Register(component);
            }
        }
    }
}