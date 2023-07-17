namespace Aristocrat.Monaco.Hardware.PWM
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Timers;
    using Hardware.Contracts;
    using Hardware.Contracts.PWM;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts.Events;
    using log4net;
    using Timer = System.Timers.Timer;

    public class CoinAcceptorService : BaseRunnable, ICoinAcceptorService
    {
        private const string DeviceImplementationsExtensionPath = "/Hardware/CoinAcceptorDevice";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Timer PollTimer = new Timer();
        private static readonly AutoResetEvent Poll = new AutoResetEvent(true);
        private readonly IEventBus _bus;
        private readonly IPropertiesManager _properties;
        protected ICoinAcceptor _acceptor;
        private readonly DeviceAddinHelper _addinHelper = new DeviceAddinHelper();
        private CoinAcceptorState _acceptorState = new CoinAcceptorState();
        private CoinEntryState _coinEntryState = new CoinEntryState();
        private readonly object _lock = new();
        private bool _coinAcceptorEnable;
        private ManualResetEvent _startupWaiter = new(false);
        private bool _disposed;

        //TODO Token value will be fetched from coin Acceptor configuration page.
        // Token value will be set via coin acceptor configuration. will be removed later
        private long _tokenValue = 100000;

        /// <inheritdoc />
        public string ServiceProtocol { get; set; }

        public string Name => nameof(CoinAcceptorService);

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(ICoinAcceptorService) };

        public virtual bool Enabled { get; protected set; }

        public virtual bool Initialized { get; protected set; }

        public DisabledReasons ReasonDisabled { get; private set; }

        public string LastError { get; } = string.Empty;

        public DivertorState DiverterDirection => _acceptorState.DivertTo;

        public string DeviceName { get; set; }

        public CoinFaultTypes Faults { get; set; }

        public CoinAcceptorService() :
            this(ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {

        }

        public CoinAcceptorService(IPropertiesManager properties,
            IEventBus bus)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _bus.Subscribe<InitializationCompletedEvent>(this, _ => _startupWaiter.Set());
        }

        protected override void OnInitialize()
        {
            ServiceProtocol = "None";
            _acceptor = (ICoinAcceptor)_addinHelper.GetDeviceImplementationObject(
                DeviceImplementationsExtensionPath,
                ServiceProtocol);

            if (_acceptor == null)
            {
                var errorMessage = "Cannot load" + Name;
                Logger.Error(errorMessage);
                throw new ServiceException(errorMessage);
            }

            Logger.Debug($"Created CoinAcceptorDevice: {ServiceProtocol}");

            Initialized = true;

            // Initialize the device implementation.
            _acceptor.Initialize();
            //Enable reject mechanish at start, So that coin could not be accepted untill enabled
            // explicitly
            CoinRejectMechOn();

            _tokenValue = _properties.GetValue(HardwareConstants.CoinValue, 100000);
            _coinAcceptorEnable = _properties.GetValue(HardwareConstants.CoinAcceptorEnabledKey, false);
            if (!_coinAcceptorEnable)
            {
                _acceptor.StopPolling();
                return;
            }

            Reset();
            DivertMechanismOnOff();
            // Set poll timer to elapsed event.
            PollTimer.Elapsed += OnPollTimeout;
            // Set poll timer interval to implementation polling frequency and start.
            //var device = _acceptor.DeviceConfiguration;
            PollTimer.Interval = _acceptor.DeviceConfig.pollingFrequency;
            PollTimer.Start();
            DeviceName = _acceptor.Name;

            Logger.Debug(Name + " initialized");
        }

        protected override void OnRun()
        {
            if (_coinAcceptorEnable)
            {
                // wait for the InitializationCompletedEvent which indicated
                // all the components we will use have been loaded
                _startupWaiter.WaitOne();
                while (RunState == RunnableState.Running)
                {
                    Poll.WaitOne();

                    if (RunState == RunnableState.Running)
                    {
                        (var status, var record) = _acceptor.Read();

                        if (status)
                        {
                            lock (_lock)
                            {
                                if (_acceptorState.CoinTransmitTimer < CoinSignalsConsts.CoinTransitTime)
                                {
                                    _acceptorState.CoinTransmitTimer += record.elapsedSinceLastChange.QuadPart / 10000;
                                }

                                if (_acceptorState.State == AcceptorState.Accept || _acceptorState.CoinTransmitTimer < CoinSignalsConsts.CoinTransitTime)
                                {
                                    this.DataProcessor(record);
                                }
                                _acceptor.AckRead(record.changeId);

                                if ((_acceptorState.pendingDiverterAction != DivertorAction.None)
                                    && _acceptorState.CoinTransmitTimer >= CoinSignalsConsts.CoinTransitTime)
                                {
                                    if (_acceptorState.pendingDiverterAction == DivertorAction.DivertToHopper)
                                    {
                                        _acceptor.DivertorMechanishOnOff(true);
                                        _acceptorState.DivertTo = DivertorState.DivertToHopper;
                                    }
                                    else
                                    {
                                        _acceptor.DivertorMechanishOnOff(false);
                                        _acceptorState.DivertTo = DivertorState.DivertToCashbox;
                                    }

                                    _acceptorState.pendingDiverterAction = DivertorAction.None;
                                    _acceptor.RejectMechanishOnOff(_acceptorState.State != AcceptorState.Accept);
                                }
                            }
                        }

                    }
                }
            }

            _acceptor.Cleanup();
            _acceptor = null;
            Dispose(true);

        }
        private void DataProcessor(ChangeRecord record)
        {
            _coinEntryState.currentState = Cc62Signals.None;

            ProcessDiverterSignal(record);
            ProcessSenseSignal(record);
            ProcessCreditSignal(record);
            ProcessAlarmSignal(record);
        }

        private void ProcessDiverterSignal(ChangeRecord record)
        {
            if (_coinEntryState.currentState != Cc62Signals.None)
            {
                //Panic
                throw new InvalidOperationException();
            }
            _coinEntryState.currentState = Cc62Signals.SolenoidSignal;

            _coinEntryState.DivertingTo = (((int)record.newValue & (int)Cc62Signals.SolenoidSignal) != 0)
                                            ? DivertorState.DivertToCashbox
                                            : DivertorState.DivertToHopper;
        }

        private void ProcessSenseSignal(ChangeRecord record)
        {
            if (_coinEntryState.currentState != Cc62Signals.SolenoidSignal)
            {
                //Panic
                throw new InvalidOperationException();
            }

            _coinEntryState.currentState = Cc62Signals.SenseSignal;

            switch (_coinEntryState.SenseState)
            {
                case SenseSignalState.HighToLow:
                    if (((int)record.newValue & (int)Cc62Signals.SenseSignal) == 0)
                    {
                        _coinEntryState.SenseState = SenseSignalState.LowToHigh;
                        _coinEntryState.SenseTime = 0;
                        break;
                    }

                    break;
                case SenseSignalState.LowToHigh:
                    _coinEntryState.SenseTime += record.elapsedSinceLastChange.QuadPart / 10000;
                    if (_coinEntryState.SenseTime > CoinSignalsConsts.SensePulseMax)
                    {
                        _coinEntryState.SenseState = SenseSignalState.Fault;
                        _bus.Publish(new HardwareFaultEvent(CoinFaultTypes.Optic));
                        break;
                    }

                    if (((int)record.newValue & (int)Cc62Signals.SenseSignal) != 0)
                    {
                        _coinEntryState.SenseState = SenseSignalState.HighToLow;
                        if (_coinEntryState.SenseTime < CoinSignalsConsts.SensePulseMin)
                        {
                            //Noise
                            break;
                        }
                        _coinEntryState.SensePulses++;
                        _coinEntryState.SenseToCreditTime = 0;
                    }

                    break;
                case SenseSignalState.Fault:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void ProcessCreditSignal(ChangeRecord record)
        {
            if (_coinEntryState.currentState != Cc62Signals.SenseSignal)
            {
                //Panic
                throw new InvalidOperationException();
            }

            _coinEntryState.currentState = Cc62Signals.CreditSignal;
            switch (_coinEntryState.CreditState)
            {
                case CreditSignalState.HighToLow:
                    CheckCreditToSensePulse(record.elapsedSinceLastChange.QuadPart / 10000);
                    if (((int)record.newValue & (int)Cc62Signals.CreditSignal) == 0)
                    {
                        _coinEntryState.CreditState = CreditSignalState.LowToHigh;
                        _coinEntryState.CreditTime = 0;
                        break;
                    }

                    break;
                case CreditSignalState.LowToHigh:
                    CheckCreditToSensePulse(record.elapsedSinceLastChange.QuadPart / 10000);
                    _coinEntryState.CreditTime += record.elapsedSinceLastChange.QuadPart / 10000;
                    if (_coinEntryState.CreditTime > CoinSignalsConsts.CreditPulseMax)
                    {
                        _coinEntryState.CreditState = CreditSignalState.Fault;
                        _bus.Publish(new HardwareFaultEvent(CoinFaultTypes.Optic));
                        break;
                    }

                    if (((int)record.newValue & (int)Cc62Signals.CreditSignal) != 0)
                    {
                        _coinEntryState.CreditState = CreditSignalState.HighToLow;
                        if (_coinEntryState.CreditTime < CoinSignalsConsts.CreditPulseMin)
                        {
                            //Noise
                            break;
                        }

                        if (_coinEntryState.SensePulses > 0)
                        {
                            _coinEntryState.SensePulses--;
                            _bus.Publish(new CoinInEvent(new Coin() { Value = _tokenValue }));
                            if (_coinEntryState.DivertingTo == DivertorState.DivertToHopper)
                            {
                                if (_acceptorState.DivertTo == DivertorState.DivertToHopper)
                                {
                                    _bus.Publish(new CoinToHopperInEvent());
                                }
                                else
                                {
                                    _bus.Publish(new CoinToHopperInsteadOfCashboxEvent());
                                }
                            }
                            else
                            {
                                if (_coinEntryState.DivertingTo == DivertorState.DivertToCashbox)
                                {
                                    if (_acceptorState.DivertTo == DivertorState.DivertToCashbox)
                                    {
                                        _bus.Publish(new CoinToCashboxInEvent());

                                    }
                                    else
                                    {
                                        _bus.Publish(new CoinToCashboxInsteadOfHopperEvent());
                                    }
                                }
                            }
                            break;
                        }
                        _bus.Publish(new HardwareFaultEvent(CoinFaultTypes.Invalid));
                        break;
                    }

                    break;
                case CreditSignalState.Fault:
                    break;
                default:
                    throw new InvalidOperationException();
            }


        }

        private void ProcessAlarmSignal(ChangeRecord record)
        {
            if (_coinEntryState.currentState != Cc62Signals.CreditSignal)
            {
                //Panic
                throw new InvalidOperationException();
            }

            _coinEntryState.currentState = Cc62Signals.AlarmSignal;

            switch (_coinEntryState.AlarmState)
            {
                case AlarmSignalState.HighToLow:
                    if (((int)record.newValue & (int)Cc62Signals.AlarmSignal) == 0)
                    {
                        _coinEntryState.AlarmState = AlarmSignalState.LowToHigh;
                        _coinEntryState.AlarmTime = 0;
                        break;
                    }
                    break;
                case AlarmSignalState.LowToHigh:

                    _coinEntryState.AlarmTime += record.elapsedSinceLastChange.QuadPart / 10000;
                    if (_coinEntryState.AlarmTime > CoinSignalsConsts.AlarmPulseMax)
                    {
                        _coinEntryState.AlarmState = AlarmSignalState.Fault;
                        _bus.Publish(new HardwareFaultEvent(CoinFaultTypes.Optic));
                        break;

                    }

                    if (((int)record.newValue & (int)Cc62Signals.AlarmSignal) != 0)
                    {
                        _coinEntryState.AlarmState = AlarmSignalState.HighToLow;
                        if (_coinEntryState.AlarmTime < CoinSignalsConsts.AlarmPulseMin)
                        {
                            //Noise
                            break;
                        }
                        _bus.Publish(new HardwareFaultEvent(CoinFaultTypes.YoYo));
                        break;
                    }
                    break;
                case AlarmSignalState.Fault:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void CheckCreditToSensePulse(Int64 time)
        {
            _coinEntryState.SenseToCreditTime += time;
            if (_coinEntryState.SenseToCreditTime > CoinSignalsConsts.SenseToCreditMaxtime)
            {
                _coinEntryState.SensePulses = 0;
            }
        }
        public bool CoinRejectMechOn()
        {
            lock (_lock)
            {
                if (_acceptorState.pendingDiverterAction == DivertorAction.None)
                {
                    _acceptor?.RejectMechanishOnOff(true);
                }

                _acceptorState.State = AcceptorState.Reject;
                return true;
            }
        }

        public bool CoinRejectMechOff()
        {
            lock (_lock)
            {
                if (_acceptorState.pendingDiverterAction == DivertorAction.None)
                {
                    _acceptor?.RejectMechanishOnOff(false);
                }
                _acceptorState.State = AcceptorState.Accept;
                return true;
            }
        }


        public bool DivertToHopper()
        {
            lock (_lock)
            {
                if (_acceptorState.DivertTo == DivertorState.DivertToCashbox)
                {
                    _acceptor?.RejectMechanishOnOff(true);
                    _acceptorState.pendingDiverterAction = DivertorAction.DivertToHopper;
                    _acceptorState.CoinTransmitTimer = 0;

                }
                return true;
            }
        }

        public bool DivertToCashbox()
        {
            lock (_lock)
            {
                if (_acceptorState.DivertTo == DivertorState.DivertToHopper)
                {
                    _acceptor?.RejectMechanishOnOff(true);
                    _acceptorState.pendingDiverterAction = DivertorAction.DivertToCashbox;
                    _acceptorState.CoinTransmitTimer = 0;

                }
                return true;
            }
        }

        public void DivertMechanismOnOff()
        {
            //TODO: implement hopper's properties with realtime values once hopper feature is available..
            bool isHopperInstalled = true;
            bool isHopperFull = false;

            if (_properties.GetValue(HardwareConstants.HopperEnabledKey, false) && isHopperInstalled && (!isHopperFull))
            {
                DivertToHopper();
            }
            else
            {
                DivertToCashbox();
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _acceptor?.StopPolling();
                _coinEntryState.Reset();
                _acceptorState.Reset();
                _acceptor?.StartPolling();
                DivertMechanismOnOff();
            }
        }
        protected override void OnStop()
        {
            // Set poll event to unblock the runnable in order to stop.
            Poll.Set();
        }

        private static void OnPollTimeout(object sender, ElapsedEventArgs e)
        {
            // Set to poll.
            Poll.Set();
        }

        public void Disable(DisabledReasons reason)
        {
            DisabledReasons condition = 0;
            foreach (DisabledReasons value in Enum.GetValues(typeof(DisabledReasons)))
            {
                if ((reason & value) != value)          //to continue other reasons else current disable reason
                {
                    continue;
                }

                if ((ReasonDisabled & reason) > 0)      //to continue if ReasonDisabled and reason are same  
                {
                    continue;
                }

                condition |= value;
            }

            if (condition == 0)     //return in case of disable reason for which device is already disabled
            {
                return;
            }

            ReasonDisabled |= condition;        //club all the disable reasons
            Logger.Debug($"{Name} disabled by {reason}");
            Enabled = false;
            CoinRejectMechOn();
        }

        public bool Enable(EnabledReasons reason)
        {
            if (!Initialized)
            {
                Logger.Warn($"{Name} can not be enabled by {reason} because service is not initialized");
                Disable(ReasonDisabled);
                return false;
            }

            if (Enabled)
            {
                Logger.Debug($"{Name} enabled by {reason} already Enabled");
                Enabling();
                return true;
            }

            var updated = UpdateDisabledReasons(reason);

            Enabled = ReasonDisabled == 0;          //only enable when there is no disable reason exists
            Enabling();
            if (Enabled)
            {
                Logger.Debug($"{Name} enabled by {reason}");
            }
            else
            {
                Logger.Warn($"{Name} can not be enabled by {reason} because disabled by {ReasonDisabled}");
            }

            return Enabled;
        }

        protected void Enabling()
        {
            if (Enabled)
            {
                CoinRejectMechOff();
            }
            else
            {
                CoinRejectMechOn();
            }
        }

        private DisabledReasons UpdateDisabledReasons(EnabledReasons reason)
        {
            DisabledReasons remedy = 0;

            switch (reason)
            {
                case EnabledReasons.Service:
                    remedy |= DisabledReasons.Service;
                    break;
                case EnabledReasons.Configuration:
                    remedy |= DisabledReasons.Configuration;
                    break;
                case EnabledReasons.System:
                    remedy |= DisabledReasons.System;
                    break;
                case EnabledReasons.Operator:
                    remedy |= DisabledReasons.Operator | DisabledReasons.Error | DisabledReasons.FirmwareUpdate;
                    break;
                case EnabledReasons.Reset:
                    remedy |= DisabledReasons.Error | DisabledReasons.FirmwareUpdate;
                    break;
                case EnabledReasons.Backend:
                    remedy |= DisabledReasons.Backend;
                    break;
                case EnabledReasons.Device:
                    remedy |= DisabledReasons.Device;
                    break;
                case EnabledReasons.GamePlay:
                    remedy |= DisabledReasons.GamePlay;
                    break;
            }

            var updated = ReasonDisabled & remedy;     //ex: if ReasonDisabled is 'System|Error' and remedy is 'Error' (current enable reason) then updated will be 'System'
            ReasonDisabled &= ~updated;                             //ReasonDisabled will be set current enable reason
            return updated;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {

                if (_startupWaiter != null)
                {
                    _startupWaiter.Close();
                    _startupWaiter = null;
                }

                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}
