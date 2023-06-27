namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Timers;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Contracts;
    using Contracts.Localization;
    using Contracts.Tickets;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.HardMeter;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.KeySwitch;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Models;
    using Monaco.Localization.Properties;
    using MVVM;
    using OperatorMenu;
    using DisabledEvent = Hardware.Contracts.IO.DisabledEvent;
    using EnabledEvent = Hardware.Contracts.IO.EnabledEvent;
    using Timer = System.Timers.Timer;

    [CLSCompliant(false)]
    public class IOPageViewModel : OperatorMenuPageViewModelBase
    {
        private const int RefreshInterval = 2000; // 2 seconds
        private const int MaxCharPerInputLine = 39; // 4 bits * 8 + 7 spaces
        private const int IntrusionBitsLength = 7;
        private const int NumberOfLogicalInputs = 32;
        private const int NumberOfLogicalOutputs = 32;
        private const int NumberOfPhysicalInputs = 48;
        private const int NumberOfPhysicalOutputs = 32;

        private static ulong _currentInputs;
        private static ulong _currentOutputs;
        private static int _maxInputs;
        private static int _maxOutputs;

        private readonly Timer _refreshTimer = new Timer();
        private readonly object _eventLock = new object();

        private readonly Dictionary<int, InputLabel> _logicalInputLabelDictionary = new Dictionary<int, InputLabel>();
        private readonly Dictionary<int, OutputButton> _logicalOutputButtonDictionary = new Dictionary<int, OutputButton>();
        private readonly Dictionary<int, InputLabel> _physicalInputLabelDictionary = new Dictionary<int, InputLabel>();
        private readonly Dictionary<int, OutputButton> _physicalOutputButtonDictionary = new Dictionary<int, OutputButton>();

        private bool _eventHandlerStopped;
        private string _manufacturerText;
        private string _modelText;
        private string _outputsText;
        private string _inputsText;
        private string _intrusionText;
        private Brush _statusForegroundBrush;
        private string _statusText;
        private Brush _stateForegroundBrush;
        private string _stateText;
        private IOPageStatus _status;

        public IOPageViewModel()
        {
            if (ServiceManager.GetInstance().IsServiceAvailable<IIO>() == false)
            {
                Logger.InfoFormat("IO service unavailable!");

                return;
            }

            var io = ServiceManager.GetInstance().GetService<IIO>();

            _maxInputs = io.GetMaxInputs;
            _maxOutputs = io.GetMaxOutputs;
            _currentInputs = io.GetInputs;
            _currentOutputs = io.GetOutputs;

            EventBus.Subscribe<OperatorCultureChangedEvent>(this, HandleEvent);

            InitInputLabelContent();
        }

        public string ManufacturerText
        {
            get => _manufacturerText;
            set
            {
                _manufacturerText = value;
                RaisePropertyChanged(nameof(ManufacturerText));
            }
        }

        public string ModelText
        {
            get => _modelText;
            set
            {
                _modelText = value;
                RaisePropertyChanged(nameof(ModelText));
            }
        }

        public string StateText
        {
            get => Localizer.For(CultureFor.Operator).GetString(_stateText);
            set
            {
                _stateText = value;
                RaisePropertyChanged(nameof(StateText));
            }
        }

        public Brush StateForegroundBrush
        {
            get => _stateForegroundBrush;
            set
            {
                _stateForegroundBrush = value;
                RaisePropertyChanged(nameof(StateForegroundBrush));
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                RaisePropertyChanged(nameof(StatusText));
                RaisePropertyChanged(nameof(FormattedStatus));
            }
        }


        public IOPageStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                RaisePropertyChanged(nameof(Status));
                RaisePropertyChanged(nameof(FormattedStatus));
            }
        }

        public string FormattedStatus => Status.FormattedStatus;

        public Brush StatusForegroundBrush
        {
            get => _statusForegroundBrush;
            set
            {
                _statusForegroundBrush = value;
                RaisePropertyChanged(nameof(StatusForegroundBrush));
            }
        }

        public string InputsText
        {
            get => _inputsText;
            set
            {
                _inputsText = value;
                IntrusionText = _inputsText.Substring(_inputsText.Length - IntrusionBitsLength);
                RaisePropertyChanged(nameof(InputsText));
            }
        }

        public string IntrusionText
        {
            get => _intrusionText;
            set
            {
                _intrusionText = value;
                RaisePropertyChanged(nameof(IntrusionText));
            }
        }


        public string OutputsText
        {
            get => _outputsText;
            set
            {
                _outputsText = value;
                RaisePropertyChanged(nameof(OutputsText));
            }
        }

        protected override void DisposeInternal()
        {
            StopEventHandler();
            _refreshTimer.Dispose();

            base.DisposeInternal();
        }

        private static long GetHardMeterPending(int hardMeterId)
        {
            return ServiceManager.GetInstance().TryGetService<IHardMeter>()?.GetHardMeterValue(hardMeterId) ?? 0;
        }

        private void OnRefreshTimeout(object sender, ElapsedEventArgs e)
        {
            DispatchUpdateScreen();
        }

        /// <summary>
        ///     Subscribes to events and kicks off the UI refresh timer.  The OnRefreshTimeout method, which is called
        ///     each time the timer elapses, will dispatch a call to UpdateScreen, which processes events retrieved from
        ///     the event bus.  UpdateScreen cannot run concurrently with StartEventHandler or StopEventHandler to prevent
        ///     the scenario where UpdateScreen tries to get events and this object is not subscribed to any events.
        /// </summary>
        private void StartEventHandler()
        {
            lock (_eventLock)
            {
                _eventHandlerStopped = false;
                SubscribeToEvents();

                var device = ServiceManager.GetInstance().TryGetService<IIO>();
                if (device != null)
                {
                    _refreshTimer.Interval = device.DeviceConfiguration.PollingFrequency < RefreshInterval
                        ? RefreshInterval
                        : device.DeviceConfiguration.PollingFrequency;
                }
                else
                {
                    _refreshTimer.Interval = RefreshInterval;
                }

                _refreshTimer.Start();
            }
        }

        private void StopEventHandler()
        {
            lock (_eventLock)
            {
                if (_eventHandlerStopped)
                {
                    return;
                }

                Logger.Debug("Stopping event handler");
                _eventHandlerStopped = true;

                if (_refreshTimer != null)
                {
                    _refreshTimer.Elapsed -= OnRefreshTimeout;
                    _refreshTimer.Stop();
                }
            }
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<DisabledEvent>(this, HandleEvent);
            EventBus.Subscribe<EnabledEvent>(this, HandleEvent);
            EventBus.Subscribe<InputEvent>(this, HandleEvent);
            EventBus.Subscribe<OutputEvent>(this, HandleEvent);
            EventBus.Subscribe<DownEvent>(this, HandleEvent);
            EventBus.Subscribe<UpEvent>(this, HandleEvent);
            EventBus.Subscribe<OpenEvent>(this, HandleEvent);
            EventBus.Subscribe<ClosedEvent>(this, HandleEvent);
            EventBus.Subscribe<Hardware.Contracts.HardMeter.OnEvent>(this, HandleEvent);
            EventBus.Subscribe<Hardware.Contracts.HardMeter.OffEvent>(this, HandleEvent);
            EventBus.Subscribe<Hardware.Contracts.KeySwitch.OnEvent>(this, HandleEvent);
            EventBus.Subscribe<Hardware.Contracts.KeySwitch.OffEvent>(this, HandleEvent);
            EventBus.Subscribe<ErrorEvent>(this, HandleEvent);
        }

        private void DispatchUpdateScreen()
        {
            var op = Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(UpdateScreen));

            var aborted = false;
            var status = op.Status;
            while (status != DispatcherOperationStatus.Completed && !aborted)
            {
                status = op.Wait(TimeSpan.FromSeconds(1.0));
                if (status == DispatcherOperationStatus.Aborted)
                {
                    Logger.Info("Dispatch update screen aborted");
                    aborted = true;
                }
            }
        }

        private void HandleEvent(IEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(
                () => { HandleEvents(theEvent); });
        }

        private void HandleEvents(IEvent theEvent)
        {
            if (typeof(DisabledEvent) == theEvent.GetType())
            {
                var disabledEvent = (DisabledEvent)theEvent;

                if (disabledEvent.Reasons != DisabledReasons.Error)
                {
                    Status = new IOPageStatus(ResourceKeys.DisabledByText, disabledEvent.Reasons.ToString());
                    StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByText) + disabledEvent.Reasons;
                    StatusForegroundBrush = Brushes.Yellow;
                }
                else
                {
                    StatusForegroundBrush = Brushes.Red;
                }

                Logger.DebugFormat("Handled IO.DisabledEvent {0}", disabledEvent.Reasons);
            }
            else if (typeof(EnabledEvent) == theEvent.GetType())
            {
                var enabledEvent = (EnabledEvent)theEvent;
                Status = new IOPageStatus(ResourceKeys.EnabledByText, enabledEvent.Reasons.ToString());
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledByText) + enabledEvent.Reasons;
                StatusForegroundBrush = Brushes.White;

                Logger.Debug("Handled IO.EnabledEvent");
            }
            else if (typeof(InputEvent) == theEvent.GetType())
            {
                var inEvent = (InputEvent)theEvent;

                Status = new IOPageStatus(ResourceKeys.InputEventText,
                                          inEvent.Id.ToString(),
                                          inEvent.Action ?
                                              ResourceKeys.OnText :
                                              ResourceKeys.OffText);
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InputEventText) + " " +
                              inEvent.Id.ToString(CultureInfo.CurrentCulture) +
                              string.Format(
                                  CultureInfo.CurrentCulture,
                                  " {0}",
                                  inEvent.Action ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText));
                StatusForegroundBrush = Brushes.White;

                Logger.DebugFormat(
                    "Handled InputEvent {0} action {1}",
                    inEvent.Id.ToString(CultureInfo.CurrentCulture),
                    inEvent.Action.ToString(CultureInfo.CurrentCulture));
            }
            else if (typeof(OutputEvent) == theEvent.GetType())
            {
                var inEvent = (OutputEvent)theEvent;

                Status = new IOPageStatus(ResourceKeys.OutputEventText,
                                          inEvent.Id.ToString(),
                                          inEvent.Action ?
                                              ResourceKeys.OnText :
                                              ResourceKeys.OffText);
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutputEventText) + " " +
                              inEvent.Id.ToString(CultureInfo.CurrentCulture) +
                              string.Format(
                                  CultureInfo.CurrentCulture,
                                  " {0}",
                                  inEvent.Action ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText));
                StatusForegroundBrush = Brushes.White;

                Logger.DebugFormat(
                    "Handled OutputEvent {0} action {1}",
                    inEvent.Id.ToString(CultureInfo.CurrentCulture),
                    inEvent.Action.ToString(CultureInfo.CurrentCulture));
            }
            else if (typeof(ErrorEvent) == theEvent.GetType())
            {
                var errorEvent = (ErrorEvent)theEvent;
                var id = errorEvent.Id;

                Status = new IOPageStatus(ResourceKeys.OutputEventText, id.ToString());
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutputEventText) + " " + id;
                StatusForegroundBrush = Brushes.Red;

                Logger.ErrorFormat("IO.ErrorEvent {0}", errorEvent.Id);
            }
            else if (typeof(DownEvent) == theEvent.GetType())
            {
                var buttonEvent = (DownEvent)theEvent;

                var physicalLabel = "?";
                if (_logicalInputLabelDictionary.TryGetValue(buttonEvent.LogicalId, out var value))
                {
                    physicalLabel = value.Id.ToString(CultureInfo.CurrentCulture);
                }

                Status = new IOPageStatus(ResourceKeys.PhysicalText,
                                          physicalLabel,
                                          ResourceKeys.OnText,
                                          ResourceKeys.LogicalText,
                                          buttonEvent.LogicalId.ToString(),
                                          ResourceKeys.ButtonsLabel,
                                          ResourceKeys.DownText);
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PhysicalText) + " " + physicalLabel + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText) +
                              " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicalText) + " " + buttonEvent.LogicalId + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ButtonsLabel) + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DownText);
                StatusForegroundBrush = Brushes.White;

                SetLogicalInputLabelState(buttonEvent.LogicalId, true, true);

                Logger.DebugFormat(
                    "Handled DownEvent LogicalID {0}",
                    buttonEvent.LogicalId.ToString(CultureInfo.CurrentCulture));
            }
            else if (typeof(UpEvent) == theEvent.GetType())
            {
                var buttonEvent = (UpEvent)theEvent;

                var physicalLabel = "?";
                if (_logicalInputLabelDictionary.TryGetValue(buttonEvent.LogicalId, out var value))
                {
                    physicalLabel = value.Id.ToString(CultureInfo.CurrentCulture);
                }

                Status = new IOPageStatus(ResourceKeys.PhysicalText,
                                          physicalLabel,
                                          ResourceKeys.OnText,
                                          ResourceKeys.LogicalText,
                                          buttonEvent.LogicalId.ToString(),
                                          ResourceKeys.ButtonsLabel,
                                          ResourceKeys.UpText);
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PhysicalText) + " " + physicalLabel + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText) +
                              " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicalText) + " " + buttonEvent.LogicalId + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ButtonsLabel) + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UpText);
                StatusForegroundBrush = Brushes.White;

                SetLogicalInputLabelState(buttonEvent.LogicalId, false, true);

                Logger.DebugFormat(
                    "Handled UpEvent LogicalID {0}",
                    buttonEvent.LogicalId.ToString(CultureInfo.CurrentCulture));
            }
            else if (typeof(OpenEvent) == theEvent.GetType())
            {
                var doorEvent = (OpenEvent)theEvent;

                UpdateStatusTextForDoor(doorEvent.LogicalId, true);

                SetLogicalInputLabelState(doorEvent.LogicalId, false, true);

                Logger.DebugFormat(
                    "Handled Door.OpenEvent LogicalID {0}",
                    doorEvent.LogicalId.ToString(CultureInfo.CurrentCulture));
            }
            else if (typeof(ClosedEvent) == theEvent.GetType())
            {
                var doorEvent = (ClosedEvent)theEvent;

                UpdateStatusTextForDoor(doorEvent.LogicalId, false);

                SetLogicalInputLabelState(doorEvent.LogicalId, true, true);

                Logger.DebugFormat(
                    "Handled ClosedEvent LogicalID {0}",
                    doorEvent.LogicalId.ToString(CultureInfo.CurrentCulture));
            }
            else if (typeof(Hardware.Contracts.HardMeter.OnEvent) == theEvent.GetType())
            {
                var hardMeterEvent = (Hardware.Contracts.HardMeter.OnEvent)theEvent;

                var physicalLabel = "?";
                if (_logicalOutputButtonDictionary.TryGetValue(hardMeterEvent.LogicalId, out var value))
                {
                    physicalLabel = value.Id.ToString(CultureInfo.CurrentCulture);
                }

                Status = new IOPageStatus(ResourceKeys.PhysicalText,
                                          physicalLabel,
                                          ResourceKeys.OnText,
                                          ResourceKeys.LogicalText,
                                          hardMeterEvent.LogicalId.ToString(),
                                          ResourceKeys.HardMeterLabel,
                                          ResourceKeys.OnText);
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PhysicalText) + " " + physicalLabel + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText) +
                              " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicalText) + " " + hardMeterEvent.LogicalId + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardMeterLabel) + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText);
                StatusForegroundBrush = Brushes.White;

                Logger.DebugFormat(
                    "Handled HardMeter.OnEvent LogicalID {0}",
                    hardMeterEvent.LogicalId.ToString(CultureInfo.CurrentCulture));
            }
            else if (typeof(Hardware.Contracts.HardMeter.OffEvent) == theEvent.GetType())
            {
                var hardMeterEvent = (Hardware.Contracts.HardMeter.OffEvent)theEvent;

                var physicalLabel = "?";
                if (_logicalOutputButtonDictionary.TryGetValue(hardMeterEvent.LogicalId, out var value))
                {
                    physicalLabel = value.Id.ToString(CultureInfo.CurrentCulture);
                }

                Status = new IOPageStatus(ResourceKeys.PhysicalText,
                                          physicalLabel,
                                          ResourceKeys.OffText,
                                          ResourceKeys.LogicalText,
                                          hardMeterEvent.LogicalId.ToString(),
                                          ResourceKeys.HardMeterLabel,
                                          ResourceKeys.OffText);
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PhysicalText) + " " + physicalLabel + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText) +
                              " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicalText) + " " + hardMeterEvent.LogicalId + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardMeterLabel) + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText);
                StatusForegroundBrush = Brushes.White;

                Logger.DebugFormat(
                    "Handled HardMeter.OnEvent LogicalID {0}",
                    hardMeterEvent.LogicalId.ToString(CultureInfo.CurrentCulture));
            }
            else if (typeof(Hardware.Contracts.KeySwitch.OnEvent) == theEvent.GetType())
            {
                var keySwitchEvent = (Hardware.Contracts.KeySwitch.OnEvent)theEvent;

                var physicalLabel = "?";
                if (_logicalInputLabelDictionary.TryGetValue(keySwitchEvent.LogicalId, out var value))
                {
                    physicalLabel = value.Id.ToString(CultureInfo.CurrentCulture);
                }

                Status = new IOPageStatus(ResourceKeys.PhysicalText,
                                          physicalLabel,
                                          ResourceKeys.OffText,
                                          ResourceKeys.LogicalText,
                                          keySwitchEvent.LogicalId.ToString(),
                                          ResourceKeys.KeySwitchText,
                                          ResourceKeys.OffText);
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PhysicalText) + " " + physicalLabel + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText) +
                              " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicalText) + " " + keySwitchEvent.LogicalId + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.KeySwitchText) + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText);
                StatusForegroundBrush = Brushes.White;

                SetLogicalInputLabelState(keySwitchEvent.LogicalId, false, true);

                Logger.DebugFormat(
                    "Handled KeySwitch.OnEvent LogicalID {0}",
                    keySwitchEvent.LogicalId.ToString(CultureInfo.CurrentCulture));
            }
            else if (typeof(Hardware.Contracts.KeySwitch.OffEvent) == theEvent.GetType())
            {
                var keySwitchEvent = (Hardware.Contracts.KeySwitch.OffEvent)theEvent;

                var physicalLabel = "?";
                if (_logicalInputLabelDictionary.TryGetValue(keySwitchEvent.LogicalId, out var value))
                {
                    physicalLabel = value.Id.ToString(CultureInfo.CurrentCulture);
                }

                Status = new IOPageStatus(ResourceKeys.PhysicalText,
                                          physicalLabel,
                                          ResourceKeys.OnText,
                                          ResourceKeys.LogicalText,
                                          keySwitchEvent.LogicalId.ToString(),
                                          ResourceKeys.KeySwitchText,
                                          ResourceKeys.OnText);
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PhysicalText) + " " + physicalLabel + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText) +
                              " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicalText) + " " + keySwitchEvent.LogicalId + " " +
                              Localizer.For(CultureFor.Operator).GetString(ResourceKeys.KeySwitchText) + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText);
                StatusForegroundBrush = Brushes.White;

                SetLogicalInputLabelState(keySwitchEvent.LogicalId, true, true);

                Logger.DebugFormat(
                    "Handled KeySwitch.OffEvent LogicalID {0}",
                    keySwitchEvent.LogicalId.ToString(CultureInfo.CurrentCulture));
            }
            else
            {
                Logger.ErrorFormat(CultureInfo.CurrentCulture, "Unexpected event type {0}", theEvent.GetType());

                Status = new IOPageStatus(ResourceKeys.UnexpectedEventText, theEvent.GetType().ToString());
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UnexpectedEventText) + " " + theEvent.GetType();
                StatusForegroundBrush = Brushes.Red;
            }
        }

        private void HandleEvent(OperatorCultureChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                RaisePropertyChanged(nameof(StatusText));
                RaisePropertyChanged(nameof(StateText));
                RaisePropertyChanged(nameof(FormattedStatus));
            });
        }

        private void InitInputLabelContent()
        {
            // Set physical input labels first.
            for (int i = 0; i < NumberOfPhysicalInputs; i++)
            {
                _physicalInputLabelDictionary.Add(i, new InputLabel(-1));
            }

            // Set logical input labels second. Depends upon physical input labels for logical Id assignment.
            for (int i = 0; i < NumberOfLogicalInputs; i++)
            {

                _logicalInputLabelDictionary.Add(i, GetInputLabel(i));
            }
        }

        private void InitOutputButtonContent()
        {
            _physicalOutputButtonDictionary.Clear();
            // Set physical output buttons first.
            for (int i = 0; i < NumberOfPhysicalOutputs; i++)
            {
                _physicalOutputButtonDictionary.Add(i, new OutputButton(-1));
            }

            _logicalOutputButtonDictionary.Clear();
            // Set logical output buttons second. Depends upon physical output buttons for logical Id assignment.
            for (int i = 0; i < NumberOfLogicalOutputs; i++)
            {
                _logicalOutputButtonDictionary.Add(i, GetOutputButton(i));
            }
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            var serviceManager = ServiceManager.GetInstance();

            var ticketCreator = serviceManager.TryGetService<IInformationTicketCreator>();
            var printer = serviceManager.TryGetService<IPrinter>();

            if (ticketCreator == null || printer == null)
            {
                return null;
            }

            string title = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IOConfigScreen);

            var tempString = string.Empty;

            tempString += Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ManufacturerLabel) + ": ";
            tempString += ManufacturerText;
            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ModelLabel) + ": ";
            tempString += ModelText;

            tempString += "\n \n";
            tempString += Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StateLabel) + ": ";
            tempString += StateText;

            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StatusLabel) + ": \n";
            tempString += StatusText;

            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InputsLabel) + ": \n";
            var inputs = InputsText.Trim();

            if (inputs.Length > MaxCharPerInputLine)
            {
                var intrusionBits = inputs.Substring(inputs.Length - IntrusionBitsLength);
                tempString += inputs.Substring(0, MaxCharPerInputLine);
                tempString += "\n" + inputs.Substring(MaxCharPerInputLine).Trim();
                tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IntrusionText) + ": \n";
                tempString += intrusionBits;
            }
            else
            {
                tempString += inputs;
            }

            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutputsLabel) + ": \n";
            var outputs = OutputsText.Trim();

            tempString += outputs;

            tempString += "\n \n";
            var testTemp = string.Empty;
            ReportComponentInformation(ref testTemp);
            tempString += testTemp;

            return TicketToList(ticketCreator.CreateInformationTicket(title, tempString));
        }

        private void ReportComponentInformation(ref string reportData)
        {
            if (ServiceManager.GetInstance().IsServiceAvailable<IIO>() == false)
            {
                Logger.InfoFormat("IO service unavailable!");
                return;
            }

            var io = ServiceManager.GetInstance().GetService<IIO>();

            var currentDir = Directory.GetCurrentDirectory();

            var componentFiles = Directory.GetFiles(currentDir, "IO*.dll");
            reportData = string.Empty;
            foreach (var file in componentFiles)
            {
                var assembly = Assembly.LoadFile(file);
                var assemblyName = assembly.GetName();
                if (assemblyName.Name.Contains("Interfaces"))
                {
                    reportData += assemblyName.Name + " " + assemblyName.Version;
                    reportData += "\n";
                }
                else if (assemblyName.Name.Contains("Factory"))
                {
                    reportData += assemblyName.Name + " " + assemblyName.Version;
                    reportData += "\n";
                }
                else if (assemblyName.Name.Contains("Service"))
                {
                    reportData += assemblyName.Name + " " + assemblyName.Version;
                    reportData += "\n";
                }
                else if (io.LogicalState != IOLogicalState.Uninitialized)
                {
                    var device = io.DeviceConfiguration;
                    if (assemblyName.Name.Contains(device.Protocol))
                    {
                        reportData += assemblyName.Name + " " + assemblyName.Version;
                        reportData += "\n";
                    }
                }
            }

            var wpfFiles = Directory.GetFiles(currentDir, "WpfIO*.dll");

            foreach (var file in wpfFiles)
            {
                var assembly = Assembly.LoadFile(file);
                var assemblyName = assembly.GetName();
                if (assemblyName.Name.Contains("Screen"))
                {
                    reportData += assemblyName.Name + " " + assemblyName.Version;
                    reportData += "\n";
                }
            }
        }

        /// <summary>Gets the input physical ID associated with the given logical ID.</summary>
        /// <param name="logicalId">Logical ID</param>
        /// <returns>Physical ID</returns>
        private InputLabel GetInputLabel(int logicalId)
        {
            var physicalId = -1;

            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(0);
            var methodBase = stackFrame.GetMethod();

            string content = logicalId + " Off";

            if (ServiceManager.GetInstance().IsServiceAvailable<IButtonService>())
            {
                var button = ServiceManager.GetInstance().GetService<IButtonService>();
                physicalId = button.GetButtonPhysicalId(logicalId);

                if (physicalId > -1)
                {
                    content = logicalId + " " + button.GetLocalizedButtonName(logicalId, Localizer.For(CultureFor.Operator).GetString) + " " +
                              (button.GetButtonAction(logicalId) == ButtonAction.Down
                                  ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DownText)
                                  : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UpText));

                    if (physicalId < _physicalInputLabelDictionary.Count)
                    {
                        _physicalInputLabelDictionary[physicalId].Id = logicalId;
                        Logger.InfoFormat(
                            "{0} physical ID {1} assigned to btn {2}",
                            methodBase.Name,
                            physicalId,
                            logicalId);
                    }
                    else
                    {
                        Logger.WarnFormat("{0} physical ID {1} not handled", methodBase.Name, physicalId);
                    }
                }
            }

            if (physicalId == -1 && ServiceManager.GetInstance().IsServiceAvailable<IDoorService>())
            {
                var door = ServiceManager.GetInstance().GetService<IDoorService>();
                var monitor = ServiceManager.GetInstance().GetService<IDoorMonitor>();
                physicalId = door.GetDoorPhysicalId(logicalId);

                if (physicalId > -1)
                {
                    content = logicalId + " " + monitor.GetLocalizedDoorName(logicalId) + " " +
                              (!door.GetDoorClosed(logicalId)
                                  ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenText)
                                  : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClosedText));

                    if (physicalId < _physicalInputLabelDictionary.Count)
                    {
                        _physicalInputLabelDictionary[physicalId].Id = logicalId;
                        Logger.InfoFormat(
                            "{0} physical ID {1} assigned to logical door {2}",
                            methodBase.Name,
                            physicalId,
                            logicalId);
                    }
                    else
                    {
                        Logger.WarnFormat("{0} physical ID {1} not handled", methodBase.Name, physicalId);
                    }
                }
            }

            if (physicalId == -1 && ServiceManager.GetInstance().IsServiceAvailable<IKeySwitch>())
            {
                var keySwitch = ServiceManager.GetInstance().GetService<IKeySwitch>();
                physicalId = keySwitch.GetKeySwitchPhysicalId(logicalId);

                if (physicalId > -1)
                {
                    content = logicalId + " " + keySwitch.GetLocalizedKeySwitchName(logicalId) + " " +
                              (keySwitch.GetKeySwitchAction(logicalId) == KeySwitchAction.On
                                  ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText)
                                  : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText));

                    if (physicalId < _physicalInputLabelDictionary.Count)
                    {
                        _physicalInputLabelDictionary[physicalId].Id = logicalId;
                        Logger.InfoFormat(
                            "{0} physical ID {1} assigned to logical key switch {2}",
                            methodBase.Name,
                            physicalId,
                            logicalId);
                    }
                    else
                    {
                        Logger.WarnFormat("{0} physical ID {1} not handled", methodBase.Name, physicalId);
                    }
                }
            }

            return new InputLabel(physicalId) { Content = content };
        }

        /// <summary>Gets the output physical ID associated with the given logical ID.</summary>
        /// <param name="logicalId">Logical ID</param>
        /// <returns>Physical ID</returns>
        private OutputButton GetOutputButton(int logicalId)
        {
            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(0);
            var methodBase = stackFrame.GetMethod();

            var button = new OutputButton(logicalId);

            if (ServiceManager.GetInstance().IsServiceAvailable<IHardMeter>())
            {
                var hardMeter = ServiceManager.GetInstance().GetService<IHardMeter>();
                var physicalId = hardMeter.GetHardMeterPhysicalId(logicalId);

                if (physicalId > -1)
                {
                    button.Id = physicalId;

                    button.Content = string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} {1} {2} {3} {4}",
                        logicalId,
                        hardMeter.LogicalHardMeters[logicalId].Suspended
                            ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ResumeText)
                            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SuspendText),
                        hardMeter.GetLocalizedHardMeterName(logicalId),
                        GetHardMeterPending(logicalId),
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Pending));

                    if (physicalId < _physicalOutputButtonDictionary.Count)
                    {
                        _physicalOutputButtonDictionary[physicalId].Id = logicalId;
                        _physicalOutputButtonDictionary[physicalId].IsEnabled =
                            false; // Disallow physical hard meter button usage
                        Logger.DebugFormat(
                            "{0} physical ID {1} assigned to logical button {2}",
                            methodBase.Name,
                            physicalId,
                            logicalId);
                    }
                    else
                    {
                        Logger.ErrorFormat("{0} physical ID {1} not handled", methodBase.Name, physicalId);
                    }
                }
            }

            return button;
        }

        private void SetStateInformation()
        {
            if (ServiceManager.GetInstance().IsServiceAvailable<IIO>() == false)
            {
                Logger.InfoFormat("IO service unavailable");
                return;
            }

            var io = ServiceManager.GetInstance().GetService<IIO>();

            StateText = io.LogicalState.ToString();

            switch (io.LogicalState)
            {
                case IOLogicalState.Disabled:
                case IOLogicalState.Error:
                    StateForegroundBrush = Brushes.Red;
                    break;
                case IOLogicalState.Uninitialized:
                    StateForegroundBrush = Brushes.Gray;
                    break;
                default:
                    StateForegroundBrush = Brushes.White;
                    break;
            }
        }

        private void SetLogicalInputLabelState(int logicalId, bool logicalOn, bool isLogicalEvent)
        {
            if (!_logicalInputLabelDictionary.TryGetValue(logicalId, out var value))
            {
                return;
            }
            var physicalOn = ((long)_currentInputs & ((long)1 << value.Id)) != 0;

            var on = logicalOn;

            if (isLogicalEvent == false)
            {
                on = physicalOn;
            }

            var content = _logicalInputLabelDictionary[logicalId].Content;

            if (on)
            {
                if (content.Contains(" " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText)))
                {
                    _logicalInputLabelDictionary[logicalId].Content = content.Replace(
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText),
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText));
                }
                else if (content.Contains(" " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenText)))
                {
                    _logicalInputLabelDictionary[logicalId].Content = content.Replace(
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenText),
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClosedText));
                }
                else if (content.Contains(" " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UpText)))
                {
                    _logicalInputLabelDictionary[logicalId].Content = content.Replace(
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UpText),
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DownText));
                }
            }
            else
            {
                if (content.Contains(" " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText)))
                {
                    _logicalInputLabelDictionary[logicalId].Content = content.Replace(
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText),
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText));
                }
                else if (content.Contains(" " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClosedText)))
                {
                    _logicalInputLabelDictionary[logicalId].Content = content.Replace(
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClosedText),
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenText));
                }
                else if (content.Contains(" " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DownText)))
                {
                    _logicalInputLabelDictionary[logicalId].Content = content.Replace(
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DownText),
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UpText));
                }
            }
        }

        private void SetLogicalOutputButtonState(int logicalId, bool logicalOn, bool set)
        {
            if (!_logicalOutputButtonDictionary.TryGetValue(logicalId, out var value))
            {
                return;
            }

            var physicalId = value.Id;
            var physicalOn = ((long)_currentOutputs & ((long)1 << physicalId)) != 0;

            var on = logicalOn;

            if (set == false)
            {
                on = physicalOn;
            }

            if (physicalId > -1)
            {
                if (ServiceManager.GetInstance().IsServiceAvailable<IHardMeter>())
                {
                    var hardMeter = ServiceManager.GetInstance().GetService<IHardMeter>();
                    if (physicalId == hardMeter.GetHardMeterPhysicalId(logicalId))
                    {
                        // Set suspend/resume button content and pending tick count.
                        if (on)
                        {
                            _logicalOutputButtonDictionary[logicalId].Content =
                                logicalId + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ResumeText) + " " +
                                hardMeter.LogicalHardMeters[logicalId].LocalizedName + " " +
                                GetHardMeterPending(logicalId) + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Pending);
                        }
                        else
                        {
                            _logicalOutputButtonDictionary[logicalId].Content =
                                logicalId + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SuspendText) + " " +
                                hardMeter.LogicalHardMeters[logicalId].LocalizedName + " " +
                                GetHardMeterPending(logicalId) + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Pending);
                        }
                    }
                }
            }

            var content = _logicalOutputButtonDictionary[logicalId].Content;

            if (on)
            {
                if (content.Contains(" " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText)))
                {
                    _logicalOutputButtonDictionary[logicalId].Content = content.Replace(
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText),
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText));
                }
            }
            else
            {
                if (content.Contains(" " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText)))
                {
                    _logicalOutputButtonDictionary[logicalId].Content = content.Replace(
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText),
                        " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText));
                }
            }
        }

        private void SetPhysicalInputLabelState(int physicalId, bool on)
        {
            if (_physicalInputLabelDictionary.TryGetValue(physicalId, out var value))
            {
                value.Content = on
                    ? physicalId.ToString(CultureInfo.CurrentCulture) + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText)
                    : physicalId.ToString(CultureInfo.CurrentCulture) + " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText);
            }
        }

        private void SetPhysicalOutputButtonState(int physicalId, bool on, bool set)
        {
            if (set && ServiceManager.GetInstance().IsServiceAvailable<IIO>())
            {
                var io = ServiceManager.GetInstance().GetService<IIO>();
                io.SetOutput(physicalId, !on, true);
                return;
            }

            if (_physicalOutputButtonDictionary.TryGetValue(physicalId, out var value))
            {
                value.Content = on
                    ? physicalId.ToString(CultureInfo.CurrentCulture) + " " +
                      Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TurnText) +
                      " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText)
                    : physicalId.ToString(CultureInfo.CurrentCulture) + " " +
                      Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TurnText) +
                      " " + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText);
            }
        }

        private void SetOutputButtonEnableDisable(int logicalId, bool enabled)
        {
            var physicalId = -1;
            if (_logicalOutputButtonDictionary.TryGetValue(logicalId, out var value))
            {
                value.IsEnabled = enabled;
                physicalId = value.Id;
            }

            if (_physicalOutputButtonDictionary.TryGetValue(physicalId, out var val))
            {
                val.IsEnabled = enabled;
            }
        }

        private void UpdateStatusTextForDoor(int doorLogicId, bool open)
        {
            var physicalLabel = "?";
            if (_logicalInputLabelDictionary.TryGetValue(doorLogicId, out var value))
            {
                physicalLabel = value.Id.ToString(CultureInfo.CurrentCulture);
            }

            Status = new IOPageStatus(ResourceKeys.PhysicalText,
                                      physicalLabel,
                                      open ? ResourceKeys.OffText : ResourceKeys.OnText,
                                      ResourceKeys.LogicalText,
                                      doorLogicId.ToString(),
                                      ResourceKeys.DoorText,
                                      open ? ResourceKeys.OpenText : ResourceKeys.ClosedText);
            StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PhysicalText) + " " + physicalLabel + " " +
                         (open ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OffText) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OnText)) + " " +
                         Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicalText) + " " + doorLogicId + " " +
                         Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DoorText) + " " +
                         (open ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenText) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClosedText));
            StatusForegroundBrush = Brushes.White;

        }

        private void UpdateScreen()
        {
            lock (_eventLock)
            {
                if (_eventHandlerStopped)
                {
                    return;
                }

                if (ServiceManager.GetInstance().IsServiceAvailable<IIO>() == false)
                {
                    Logger.InfoFormat("IO service unavailable");

                    return;
                }

                var io = ServiceManager.GetInstance().GetService<IIO>();

                if (ServiceManager.GetInstance().IsServiceAvailable<IHardMeter>())
                {
                    var hardMeter = ServiceManager.GetInstance().GetService<IHardMeter>();

                    if (hardMeter.LogicalState == HardMeterLogicalState.Idle)
                    {
                        var logicalHardMeters = hardMeter.LogicalHardMeters;
                        foreach (var pair in logicalHardMeters)
                        {
                            SetLogicalOutputButtonState(pair.Key, pair.Value.Suspended, true);
                        }
                    }
                }

                SetStateInformation();

                var inputs = string.Empty;
                var previousInputs = _currentInputs;
                _currentInputs = io.GetInputs;
                for (var i = 0; i < _maxInputs; i++)
                {
                    if (i % 4 == 0)
                    {
                        inputs += " ";
                    }

                    var on = ((long)_currentInputs & ((long)1 << i)) != 0;
                    inputs += on ? "1" : "0";

                    if (((long)_currentInputs & ((long)1 << i)) != ((long)previousInputs & ((long)1 << i)))
                    {
                        SetPhysicalInputLabelState(i, on);
                    }
                }

                InputsText = inputs.Trim();

                var outputs = string.Empty;
                var previousOutputs = _currentOutputs;
                _currentOutputs = io.GetOutputs;
                for (var i = 0; i < _maxOutputs; i++)
                {
                    if (i % 4 == 0)
                    {
                        outputs += " ";
                    }

                    var on = ((long)_currentOutputs & ((long)1 << i)) != 0;
                    outputs += on ? "1" : "0";

                    if (((long)_currentOutputs & ((long)1 << i)) != ((long)previousOutputs & ((long)1 << i)))
                    {
                        SetPhysicalOutputButtonState(i, on, false);
                    }
                }

                OutputsText = outputs.Trim();
            }
        }

        protected override void OnUnloaded()
        {
            Logger.Debug("Page unloaded");
            StopEventHandler();
        }

        protected override void OnLoaded()
        {
            var io = ServiceManager.GetInstance().GetService<IIO>();
            var ioservice = (IDeviceService)ServiceManager.GetInstance().GetService<IIO>();
            var device = io.DeviceConfiguration;
            var door = ServiceManager.GetInstance().GetService<IDoorService>();
            DateTime? lastDoorOpenedDateTime = null;
            var lastDoorOpenInput = -1;

            // For max inputs...
            for (var i = 0; i < _maxInputs; i++)
            {
                // Set the logical input label state to the current associated physical input.  This will initialize
                // the logical label as if it was set from handling a logical event.
                if (Enum.IsDefined(typeof(DoorLogicalId), i))
                {
                    var isDoorClosed = door.GetDoorClosed(i);

                    // Check to see if there are any doors open currently, if so get most recent DoorId and DoorLastOpened time
                    if (!isDoorClosed)
                    {
                        if (!lastDoorOpenedDateTime.HasValue)
                        {
                            lastDoorOpenedDateTime = door.GetDoorLastOpened(i);
                            lastDoorOpenInput = i;
                        }
                        else if (lastDoorOpenedDateTime.Value < door.GetDoorLastOpened(i))
                        {
                            lastDoorOpenedDateTime = door.GetDoorLastOpened(i);
                            lastDoorOpenInput = i;
                        }
                    }

                    SetLogicalInputLabelState(i, isDoorClosed, true);
                }
                else
                {
                    SetLogicalInputLabelState(i, true, true);
                }
            }

            if (lastDoorOpenedDateTime.HasValue && lastDoorOpenInput > -1)
            {
                UpdateStatusTextForDoor(lastDoorOpenInput, true);
            }

            // Negate current inputs so that all physical input labels are updated on the first pass thru updating the screen.
            long negatedInputs = 0;
            for (var i = 0; i < (long)_maxInputs; i++)
            {
                if (((long)_currentInputs & ((long)1 << i)) == 0)
                {
                    negatedInputs |= (long)1 << i;
                }
            }

            _currentInputs = (ulong)negatedInputs;

            InitOutputButtonContent();

            // For max outputs...
            for (var i = 0; i < _maxOutputs; i++)
            {
                // Set the logical output button state to the current associated physical output.  This will initialize
                // the logical button as if it was set from handling a logical event.
                SetLogicalOutputButtonState(i, false, false);
            }

            // Negate current outputs so that all physical output buttons are updated on the first pass thru updating the screen.
            long negatedOutputs = 0;
            for (var i = 0; i < (long)_maxOutputs; i++)
            {
                if (((long)_currentOutputs & ((long)1 << i)) == 0)
                {
                    negatedOutputs |= (long)1 << i;
                }
            }

            _currentOutputs = (ulong)negatedOutputs;

            if (io.LogicalState != IOLogicalState.Uninitialized)
            {
                if (io.LogicalState == IOLogicalState.Disabled)
                {
                    if (ioservice.ReasonDisabled != DisabledReasons.Error)
                    {
                        Status = new IOPageStatus(ResourceKeys.DisabledByText, ioservice.ReasonDisabled.ToString());
                        StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByText) + ioservice.ReasonDisabled;
                        StatusForegroundBrush = Brushes.Yellow;
                    }
                    else
                    {
                        Status = new IOPageStatus("", ioservice.LastError.ToString());
                        StatusText = ioservice.LastError;
                        StatusForegroundBrush = Brushes.Red;
                    }
                }

                ManufacturerText = device.Manufacturer;
                ModelText = device.Model;
            }

            _refreshTimer.Elapsed += OnRefreshTimeout;

            StartEventHandler();

            UpdateScreen();
        }

        private class InputLabel
        {
            public InputLabel(int id)
            {
                Id = id;
                Content = id + " Off";
            }

            public int Id { get; set; }

            public string Content { get; set; }
        }

        private class OutputButton
        {
            public OutputButton(int id)
            {
                Id = id;
                Content = id + " On";
            }

            public int Id { get; set; }

            public string Content { get; set; }

            public bool IsEnabled { get; set; }
        }
    }
}
