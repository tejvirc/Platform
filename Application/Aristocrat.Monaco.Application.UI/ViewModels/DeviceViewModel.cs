namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Common;
    using Contracts;
    using Contracts.Tickets;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using OperatorMenu;
    using System;
    using System.Collections.Generic;
    using System.Timers;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Contracts.Localization;
    using Monaco.Localization.Properties;
    using Hardware.Contracts.SerialPorts;

    /// <summary>
    ///     A DeviceViewModel contains the base logic for device config page view models
    /// </summary>
    /// <seealso cref="OperatorMenuPageViewModelBase" />
    [CLSCompliant(false)]
    public abstract class DeviceViewModel : OperatorMenuPageViewModelBase
    {
        private const string CommunicatorsAddinPath = "/Hardware/CommunicatorDrivers";
        private const double DispatchWaitTimeout = 1.0;
        private const int RefreshInterval = 2000; // 2 seconds
        protected const string Usb = "USB";

        protected Timer RefreshTimer;

        protected readonly object EventLock = new object();

        private readonly DeviceAddinHelper _addinHelper = new DeviceAddinHelper();

        private readonly DeviceType _deviceType;

        protected bool EventHandlerStopped;

        private string _activationTime = string.Empty;
        private bool _activationVisible;
        private DispatcherOperation _dispatcherOperation;
        private string _firmwareCrcText;
        private string _firmwareRevisionText;
        private string _firmwareVersionText;
        private string _manufacturerText;
        private string _modelText;
        private string _portText;
        private string _portLabelContent;
        private string _protocolText;
        private bool _selfTestButtonEnabled;
        private SelfTestState _selfTestState;
        private string _serialNumberText;
        private bool _showingDiagnostics;
        private StateMode _stateMode;
        private string _stateText;
        private StatusMode _statusMode;
        private string _statusText = string.Empty;

        protected DeviceViewModel(DeviceType type) 
        {
            _deviceType = type;
            RefreshTimer = new Timer();
            RefreshTimer.Elapsed += OnRefreshTimeout;

            _portLabelContent = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Port);
            
            // Set whether the operator can reconfig with credits on the machine.
        }

        protected IInformationTicketCreator TicketCreator => ServiceManager.GetInstance().TryGetService<IInformationTicketCreator>();

        protected ISerialPortsService SerialPortsService => ServiceManager.GetInstance().TryGetService<ISerialPortsService>();
        
        public ICommand SelfTestButtonCommand { get; set; }

        public ICommand SelfTestClearButtonCommand { get; set; }

        public string ActivationTime
        {
            get => _activationTime;
            set
            {
                _activationTime = value;
                OnPropertyChanged(nameof(ActivationTime));
                OnPropertyChanged(nameof(ActivationForeground));
            }
        }

        public Brush ActivationForeground => ProtocolForeground;

        public bool ActivationVisible
        {
            get => _activationVisible;
            set
            {
                _activationVisible = value;
                OnPropertyChanged(nameof(ActivationVisible));
            }
        }

        public string FirmwareCrcText
        {
            get => _firmwareCrcText;
            set
            {
                _firmwareCrcText = value;
                OnPropertyChanged(nameof(FirmwareCrcText));
                OnPropertyChanged(nameof(FirmwareCrcForeground));
            }
        }

        public Brush FirmwareCrcForeground => Brushes.White; // FirmwareCrc does not exist in enum

        public string FirmwareVersionText
        {
            get => _firmwareVersionText;
            set
            {
                _firmwareVersionText = value;
                OnPropertyChanged(nameof(FirmwareVersionText));
                OnPropertyChanged(nameof(FirmwareVersionForeground));
            }
        }

        public Brush FirmwareVersionForeground => Brushes.White;

        public string FirmwareRevisionText
        {
            get => _firmwareRevisionText;
            set
            {
                _firmwareRevisionText = value;
                OnPropertyChanged(nameof(FirmwareRevisionText));
                OnPropertyChanged(nameof(FirmwareRevisionForeground));
            }
        }

        public Brush FirmwareRevisionForeground => Brushes.White;

        public string ManufacturerText
        {
            get => _manufacturerText;
            set
            {
                _manufacturerText = value;
                OnPropertyChanged(nameof(ManufacturerText));
                OnPropertyChanged(nameof(ManufacturerForeground));
            }
        }

        public Brush ManufacturerForeground => Brushes.White;

        public string ModelText
        {
            get => _modelText;
            set
            {
                _modelText = value;
                OnPropertyChanged(nameof(ModelText));
                OnPropertyChanged(nameof(ModelForeground));
            }
        }

        public Brush ModelForeground => Brushes.White;

        public string PortText
        {
            get => _portText;
            set
            {
                _portText = value;
                OnPropertyChanged(nameof(PortText));
                OnPropertyChanged(nameof(PortForeground));
            }
        }

        public Brush PortForeground => Brushes.White; // Port does not exist in enum

        public bool PortVisible => !string.IsNullOrEmpty(_portLabelContent);

        public string ProtocolText
        {
            get => _protocolText;
            set
            {
                _protocolText = value;
                OnPropertyChanged(nameof(ProtocolText));
                OnPropertyChanged(nameof(ProtocolForeground));
            }
        }

        public Brush ProtocolForeground => Brushes.White;

        public string SerialNumberText
        {
            get => _serialNumberText;
            set
            {
                _serialNumberText = value;
                OnPropertyChanged(nameof(SerialNumberText));
                OnPropertyChanged(nameof(SerialNumberForeground));
            }
        }

        public Brush SerialNumberForeground => Brushes.White; // SerialNumber does not exist in enum

        public bool ShowDiagnostics
        {
            get => _showingDiagnostics;

            set
            {
                if (_showingDiagnostics == value)
                {
                    return;
                }

                _showingDiagnostics = value;
                OnPropertyChanged(nameof(ShowDiagnostics));
                OnPropertyChanged(nameof(IsSelfTestVisible));
            }
        }

        public bool IsSelfTestVisible => ShowDiagnostics;

        public bool SelfTestButtonEnabled
        {
            get => _selfTestButtonEnabled &&
                (PrinterButtonsEnabled || !(_deviceType is DeviceType.Printer));
            set
            {
                _selfTestButtonEnabled = value;
                OnPropertyChanged(nameof(SelfTestButtonEnabled));
            }
        }

        public SelfTestState SelfTestCurrentState
        {
            get => _selfTestState;

            set
            {
                _selfTestState = value;
                OnPropertyChanged(nameof(SelfTestButtonEnabled));
                OnPropertyChanged(nameof(SelfTestText));
                OnPropertyChanged(nameof(SelfTestForeground));
            }
        }

        public string SelfTestText =>
            SelfTestCurrentState == SelfTestState.None
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.None)
                : SelfTestCurrentState == SelfTestState.Initial
                    ? string.Empty
                    : SelfTestCurrentState == SelfTestState.Failed
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SelfTestFailed)
                        : SelfTestCurrentState == SelfTestState.Passed
                            ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SelfTestPassed)
                            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SelfTestInProgress);

        public Brush SelfTestForeground =>
            SelfTestCurrentState == SelfTestState.Failed
                ? StatusBrushes.Error
                : SelfTestCurrentState == SelfTestState.Passed || SelfTestCurrentState == SelfTestState.Running
                    ? StatusBrushes.Processing
                    : SelfTestCurrentState == SelfTestState.None
                        ? StatusBrushes.None
                        : StatusBrushes.Test;

        public string StateText
        {
            get => _stateText;

            set
            {
                _stateText = value;
                OnPropertyChanged(nameof(StateText));
            }
        }

        public StateMode StateCurrentMode
        {
            get => _stateMode;

            set
            {
                _stateMode = value;
                OnPropertyChanged(nameof(StateForeground));
            }
        }

        public Brush StateForeground =>
            StateCurrentMode == StateMode.Error
                ? StatusBrushes.Error
                : StateCurrentMode == StateMode.Uninitialized
                    ? StatusBrushes.Uninitialized
                    : StateCurrentMode == StateMode.Processing
                        ? StatusBrushes.Processing
                        : StateCurrentMode == StateMode.Warning
                            ? StatusBrushes.Warning
                            : StatusBrushes.Normal;

        public string StatusText
        {
            get => _statusText;

            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public StatusMode StatusCurrentMode
        {
            get => _statusMode;

            set
            {
                _statusMode = value;
                OnPropertyChanged(nameof(StatusForeground));
            }
        }

        public Brush StatusForeground =>
            StatusCurrentMode == StatusMode.Error
                ? StatusBrushes.Error
                : StatusCurrentMode == StatusMode.Warning
                    ? StatusBrushes.Warning
                    : StatusCurrentMode == StatusMode.None
                        ? StatusBrushes.None
                        : StatusCurrentMode == StatusMode.Working
                            ? StatusBrushes.Processing
                            : StatusBrushes.Normal;

        /// <summary>
        /// This should be overridden to call StartEventHandler(IDeviceAdapter) with the proper device adapter
        /// </summary>
        protected abstract void StartEventHandler();

        protected abstract void SubscribeToEvents();

        protected abstract void UpdateScreen();

        protected override void OnLoaded()
        {
            Logger.Debug("Page loaded");
            StartEventHandler();
        }

        protected override void OnUnloaded()
        {
            Logger.Debug("Page unloaded");
            StopEventHandler();

            SelfTestCurrentState = SelfTestState.None;
        }

        protected override void UpdatePrinterButtons()
        {
            OnPropertyChanged(nameof(SelfTestButtonEnabled));
        }
        
        protected virtual void SetDeviceInformation(IDevice device)
        {
            ManufacturerText = device.Manufacturer;
            ModelText = device.Model;
            FirmwareVersionText = device.FirmwareId;
            FirmwareRevisionText = device.FirmwareRevision;
            FirmwareCrcText = device.FirmwareCyclicRedundancyCheck;
            SerialNumberText = device.SerialNumber;
            ProtocolText = device.Protocol;
            PortText = SetPortText();

            string SetPortText()
            {
                if (string.IsNullOrEmpty(device.Protocol) || device.Protocol.Contains(ApplicationConstants.Fake))
                {
                    return ApplicationConstants.Fake;
                }
                if (device.PortName.Contains(ApplicationConstants.SerialPortPrefix))
                {
                    return SerialPortsService?.PhysicalToLogicalName(device.PortName) ?? device.PortName;
                }
                return device.PortName;
            }
        }

        internal virtual void SetDeviceInformationUnknown()
        {
            ManufacturerText = "?";
            ModelText = "?";
            FirmwareVersionText = "?";
            FirmwareRevisionText = "?";
            FirmwareCrcText = "?";
            SerialNumberText = "?";
            ProtocolText = "?";
            PortText = "?";
            ActivationTime = "?";
        }

        protected virtual void StartEventHandler(IDeviceAdapter device)
        {
            lock (EventLock)
            {
                EventHandlerStopped = false;

                SubscribeToEvents();

                if (device != null)
                {
                    RefreshTimer.Interval = device.DeviceConfiguration.PollingFrequency < RefreshInterval
                        ? RefreshInterval
                        : device.DeviceConfiguration.PollingFrequency;
                }
                else
                {
                    RefreshTimer.Interval = RefreshInterval;
                }

                RefreshTimer.Start();
            }
        }

        protected virtual void StopEventHandler()
        {
            lock (EventLock)
            {
                if (EventHandlerStopped)
                {
                    return;
                }

                Logger.Debug("Stopping event handler");
                EventHandlerStopped = true;
                RefreshTimer.Stop();

                UnsubscribeFromEvents();
                _dispatcherOperation?.Abort();
            }
        }

        protected List<Ticket> CreateInformationTicket(string title, string tempString)
        {
            return TicketToList(TicketCreator?.CreateInformationTicket(title, tempString));
        }

        protected List<string> SetPortNames(IDevice device, out int index)
        {
            var sortedPorts = new List<string>();
            if (_addinHelper.DoesDeviceImplementationExist(CommunicatorsAddinPath, Usb))
            {
                sortedPorts.Add(Usb);
            }
            else
            {
                Logger.WarnFormat(
                    "No device implementations were found for {0} with extension {1}",
                    Usb,
                    CommunicatorsAddinPath);
            }

            index = 0;

            if (device != null)
            {
                foreach (var s in sortedPorts)
                {
                    if (s == device.PortName &&
                        device.Protocol.Equals("Fake", StringComparison.CurrentCultureIgnoreCase) ==
                        false)
                    {
                        PortText = device.PortName;
                        break;
                    }

                    index++;
                }
            }

            return sortedPorts;
        }

        protected void UnsubscribeFromEvents()
        {
            EventBus.UnsubscribeAll(this);
        }

        private void OnRefreshTimeout(object sender, ElapsedEventArgs e)
        {
            _dispatcherOperation = System.Windows.Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(UpdateScreen));

            var aborted = false;
            var status = _dispatcherOperation.Status;
            while (status != DispatcherOperationStatus.Completed && !aborted)
            {
                // Wait up to 1/2 of the refresh timeout for the dispatch to occur.
                status = _dispatcherOperation.Wait(TimeSpan.FromSeconds(DispatchWaitTimeout));
                if (status == DispatcherOperationStatus.Aborted)
                {
                    Logger.Debug("Dispatch display panel information aborted");
                    aborted = true;
                }
            }
        }

        protected override void DisposeInternal()
        {
            Logger.Debug("Dispose");
            _dispatcherOperation?.Abort();
            StopEventHandler();

            if (RefreshTimer != null)
            {
                RefreshTimer.Elapsed -= OnRefreshTimeout;
                RefreshTimer.Stop();
            }

            base.DisposeInternal();
        }
    }
}
