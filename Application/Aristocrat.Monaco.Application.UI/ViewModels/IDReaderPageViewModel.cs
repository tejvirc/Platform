namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Media;
    using Common;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Monaco.Localization.Properties;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class IdReaderPageViewModel : DeviceViewModel
    {
        private readonly Dictionary<string, string> _clearEventText = new Dictionary<string, string>
        {
            { typeof(SystemOkEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OKText) },
            { typeof(IdClearedEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SystemErrorText) },
            { typeof(IdPresentedEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SystemErrorText) },
            { typeof(InspectedEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectedText) },
            { typeof(SelfTestPassedEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SelfTestPassed) }
        };

        private readonly Dictionary<string, string> _errorEventText = new Dictionary<string, string>
        {
            { typeof(HardwareFaultEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardwareErrorText) },
            { typeof(InspectionFailedEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionFailedText) },
            { typeof(SelfTestFailedEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SelfTestFailed) },
            { typeof(SystemErrorEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SystemErrorText) },
            { typeof(ReadErrorEvent).ToString(), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SystemErrorText) }
        };

        private string _idCardReadData;
        private bool _updateDeviceInformation;
        private bool _wasEnabled;

        public IdReaderPageViewModel() : base(DeviceType.IdReader)
        {
            SelfTestButtonCommand = new RelayCommand<object>(OnSelfTestCmd);
            SelfTestClearButtonCommand = new RelayCommand<object>(OnSelfTestClearNvmCmd);

            SelfTestCurrentState = SelfTestState.None;
        }

        public IIdReader IdReader
        {
            get
            {
                var idReaderProvider = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>();
                return idReaderProvider?.Adapters.FirstOrDefault(a => !a.ServiceProtocol.ToLower().Contains("virtual"));
            }
        }

        public string IdCardReadData
        {
            get => _idCardReadData;

            set
            {
                if (_idCardReadData != value)
                {
                    _idCardReadData = value;
                    OnPropertyChanged(nameof(IdCardReadData));
                    OnPropertyChanged(nameof(IdCardReadDataForeground));
                }
            }
        }

        public Brush IdCardReadDataForeground =>
            IdCardReadData == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReadErrorText)
                ? Brushes.Red
                : Brushes.LightGreen;

        /// <summary>
        ///     Gets a value indicating whether the printer is uninitialized
        /// </summary>
        public bool Uninitialized => IdReader?.LogicalState == IdReaderLogicalState.Uninitialized;

        protected override void OnLoaded()
        {
            base.OnLoaded();

            if (IdReader == null)
            {
                StateText = Regex.Replace(IdReaderLogicalState.Uninitialized.ToString(), "[A-Z]", " $0");
                StatusText = string.Empty;
                return;
            }

            _wasEnabled = IdReader.Enabled;

            if (!_wasEnabled)
            {
                IdReader?.Enable(EnabledReasons.System);
            }

            _updateDeviceInformation = true;

            UpdateScreen();
        }

        protected override void OnUnloaded()
        {
            StopEventHandler();
            if (!_wasEnabled)
            {
                IdReader?.Disable(DisabledReasons.System);
            }
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            return CreateInformationTicket(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IdReaderLabel), GetIdReaderDiagnosticInfo());
        }

        private string GetIdReaderDiagnosticInfo()
        {
            var tempString = new StringBuilder();

            tempString.Append(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ManufacturerLabel) + " ");
            tempString.Append(ManufacturerText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ModelLabel) + " ");
            tempString.Append(ModelText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SerialNumberLabel) + " ");
            tempString.Append(SerialNumberText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FirmwareVersionLabel) + " ");
            tempString.Append(FirmwareVersionText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FirmwareRevisionLabel) + " ");
            tempString.Append(FirmwareRevisionText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FirmwareCRCLabel) + " ");
            tempString.Append(FirmwareCrcText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProtocolLabel) + " ");
            tempString.Append(ProtocolText);
            if (!(ProtocolText?.Contains(ApplicationConstants.Fake) ?? false))
            {
                tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Port) + " ");
                tempString.Append(PortText);
            }

            tempString.Append("\n \n");
            tempString.Append(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StateLabel) + " ");
            tempString.Append(StateText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StatusLabel) + " ");
            tempString.Append(StatusText);

            tempString.Append("\n\n");
            tempString.Append(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PlatformVersionLabel) + " ");
            tempString.Append(Assembly.GetExecutingAssembly().GetName().Version);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OSVersionLabel) + " ");
            tempString.Append($"{Environment.OSVersion.Platform}-{Environment.OSVersion.Version}");

            tempString.Append("\n\n");
            tempString.Append("Aristocrat Technologies, INC.");

            return tempString.ToString();
        }

        private async void OnSelfTestCmd(object obj)
        {
            var idReader = IdReader;
            if (idReader != null)
            {
                SelfTestCurrentState = SelfTestState.Running;

                await IdReader.SelfTest(false).ConfigureAwait(false);
            }
        }

        private async void OnSelfTestClearNvmCmd(object obj)
        {
            var idReader = IdReader;
            if (idReader != null)
            {
                SelfTestCurrentState = SelfTestState.Running;

                await IdReader.SelfTest(true).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Updates the values shown on the operator screen.
        /// </summary>
        protected override void UpdateScreen()
        {
            lock (EventLock)
            {
                if (EventHandlerStopped)
                {
                    return;
                }

                var idReader = IdReader;
                if (idReader == null)
                {
                    return;
                }

                if (Uninitialized)
                {
                    SetDeviceInformationUnknown();
                }
                else if (_updateDeviceInformation)
                {
                    _updateDeviceInformation = false;
                    UpdateStatus(IdReader);
                }

                SetStateInformation(idReader);

                OnPropertyChanged("UninitializedVisibility");

                SelfTestButtonEnabled = IdReader.Enabled && SelfTestCurrentState != SelfTestState.Running;
            }
        }

        protected override void SubscribeToEvents()
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            eventBus.Subscribe<DisabledEvent>(this, HandleEvent);
            eventBus.Subscribe<EnabledEvent>(this, HandleEvent);
            eventBus.Subscribe<InspectedEvent>(this, HandleEvent);
            eventBus.Subscribe<ConnectedEvent>(this, HandleEvent);
            eventBus.Subscribe<DisconnectedEvent>(this, HandleEvent);
            eventBus.Subscribe<HardwareFaultClearEvent>(this, HandleEvent);
            eventBus.Subscribe<HardwareFaultEvent>(this, HandleEvent);
            eventBus.Subscribe<IdClearedEvent>(this, HandleEvent);
            eventBus.Subscribe<IdPresentedEvent>(this, HandleEvent);
            eventBus.Subscribe<SystemOkEvent>(this, HandleEvent);
            eventBus.Subscribe<SystemErrorEvent>(this, HandleEvent);
            eventBus.Subscribe<InspectionFailedEvent>(this, HandleEvent);
            eventBus.Subscribe<ReadErrorEvent>(this, HandleEvent);
            eventBus.Subscribe<SelfTestFailedEvent>(this, HandleEvent);
            eventBus.Subscribe<SelfTestPassedEvent>(this, HandleEvent);
            eventBus.Subscribe<SetValidationEvent>(this, HandleEvent);
            eventBus.Subscribe<InspectionFailedEvent>(this, HandleEvent);
            eventBus.Subscribe<SystemErrorEvent>(this, HandleEvent);
            eventBus.Subscribe<ValidationRequestedEvent>(this, HandleEvent);
        }

        /// <summary>
        ///     Subscribes to events and kicks off the UI refresh timer.  The OnRefreshTimeout method, which is called
        ///     each time the timer elapses, will dispatch a call to UpdateScreen, which processes events retrieved from
        ///     the event bus.  UpdateScreen cannot run concurrently with StartEventHandler or StopEventHandler to prevent
        ///     the scenario where UpdateScreen tries to get events and this object is not subscribed to any events.
        /// </summary>
        protected override void StartEventHandler()
        {
            base.StartEventHandler(IdReader);
        }

        private void HandleEvent(IEvent theEvent)
        {
            var idReader = IdReader;
            if (idReader == null)
            {
                return;
            }

            _updateDeviceInformation = true;

            var eventType = theEvent.GetType();
            var eventString = eventType.ToString();

            if (typeof(DisabledEvent) == eventType)
            {
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByText) + IdReader.ReasonDisabled;

                // Did we disable the service for an error?
                if ((IdReader.ReasonDisabled & DisabledReasons.Error) == 0)
                {
                    // No, set status to non-error.
                    StatusCurrentMode = StatusMode.Warning;
                }
                else
                {
                    // Yes, set status to last error.
                    SetLastErrorStatus(IdReader.LastError);
                }
            }
            else if (typeof(EnabledEvent) == eventType)
            {
                var enabledEvent = (EnabledEvent)theEvent;

                // Was the Id reader enabled by a reset?
                if ((enabledEvent.Reasons & EnabledReasons.Reset) > 0 ||
                    (enabledEvent.Reasons & EnabledReasons.Operator) > 0)
                {
                    StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledLabel);
                    if (string.IsNullOrEmpty(IdReader.LastError))
                    {
                        StatusCurrentMode = StatusMode.Normal;
                    }
                    else
                    {
                        SetLastErrorStatus(IdReader.LastError);
                    }

                    return;
                }

                // Is the Id Reader initializing or inspecting?
                if (IdReader.LogicalState == IdReaderLogicalState.Validating ||
                    IdReader.LogicalState == IdReaderLogicalState.Inspecting)
                {
                    StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CardInsertedText);
                }
                else
                {
                    StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledByText) + enabledEvent.Reasons;
                }

                StatusCurrentMode = StatusMode.Normal;
            }
            else if (typeof(InspectedEvent) == eventType)
            {
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectedText);
                StatusCurrentMode = StatusMode.Normal;
            }
            else if (typeof(IdPresentedEvent) == eventType)
            {
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CardInsertedText);
                StatusCurrentMode = StatusMode.Working;
            }
            else if (typeof(IdClearedEvent) == eventType)
            {
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CardRemovedText);

                StatusCurrentMode = StatusMode.Normal;
                SetLastErrorStatus(IdReader.LastError);
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => SetIdCardReadData()));
            }
            else if (typeof(SetValidationEvent) == eventType)
            {
                var evt = theEvent as SetValidationEvent;
                if (evt?.Identity != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() => SetIdCardReadData()));
                }
            }
            else if (typeof(InspectionFailedEvent) == eventType)
            {
                UpdateStatusError(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionFailedText), false);
            }
            else if (typeof(SelfTestPassedEvent) == eventType)
            {
                SelfTestCurrentState = SelfTestState.Passed;
            }
            else if (typeof(SelfTestFailedEvent) == eventType)
            {
                SelfTestCurrentState = SelfTestState.Failed;
            }
            else if (typeof(ReadErrorEvent) == eventType)
            {
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CardInsertedText);
                IdCardReadData = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReadErrorText);
            }
            else if (_errorEventText.ContainsKey(eventString))
            {
                UpdateStatusError(_errorEventText[eventString], false);
            }
            else if (_clearEventText.ContainsKey(eventString))
            {
                UpdateStatusError(_clearEventText[eventString], true);
            }
            else if (typeof(ValidationRequestedEvent) == eventType)
            {
                var evt = theEvent as ValidationRequestedEvent;
                if (evt?.TrackData != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() => SetIdCardReadData()));
                }
            }

            if (IdReader.LogicalState == IdReaderLogicalState.Idle)
            {
                StatusCurrentMode = StatusMode.Normal;
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CardRemovedText);
            }

            if (IdReader.LogicalState == IdReaderLogicalState.Validated)
            {
                StatusCurrentMode = StatusMode.Normal;
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CardInsertedText);
            }
        }

        private void SetIdCardReadData()
        {
            IdCardReadData = IdReader?.CardData ?? string.Empty;
        }

        private void UpdateStatus(IIdReader idReader)
        {
            // VLT-6789 : check for "virtual" contained in protocol since in GDSBase
            // we create two adapters: one = "Virtual" and the other = "Virtual2" (this one is added in the adapter collection first)
            if (idReader != null && !IdReader.ServiceProtocol.ToLower().Contains("virtual"))
            {
                SetStateInformation(idReader);

                if (idReader.LogicalState != IdReaderLogicalState.Uninitialized)
                {
                    if (!idReader.Enabled)
                    {
                        // Yes, did we disable the service for an error?
                        if ((idReader.ReasonDisabled & DisabledReasons.Error) == 0)
                        {
                            // No, set status to disabled reason.
                            StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByText) + idReader.ReasonDisabled;
                            StatusCurrentMode = StatusMode.Warning;
                        }
                        else
                        {
                            // Yes, set status to last error.
                            StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByText) + idReader.ReasonDisabled;
                        }
                    }
                    else
                    {
                        SetLastErrorStatus(idReader.LastError);
                    }

                    SetDeviceInformation();
                }
                else
                {
                    SetDeviceInformationUnknown();
                    SetLastErrorStatus(idReader.LastError);
                }
            }
        }

        /// <summary>Sets state information.</summary>
        private void SetStateInformation(IIdReader idReader)
        {
            var logicalState = idReader?.LogicalState ?? IdReaderLogicalState.Disabled;

            // Insert space before capital letters
            StateText = Regex.Replace(logicalState.ToString(), "[A-Z]", " $0");

            // This only occurs when the ID Reader Hardware panel is first opened
            if (idReader != null && string.IsNullOrEmpty(StatusText))
            {
                StatusText = idReader.LogicalState == IdReaderLogicalState.BadRead ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReadErrorText)
                    : idReader.Identity == null && IdReader?.LogicalState != IdReaderLogicalState.Validating ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CardRemovedText)
                    : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CardInsertedText);
            }

            switch (logicalState)
            {
                case IdReaderLogicalState.Disabled:
                case IdReaderLogicalState.Error:
                case IdReaderLogicalState.BadRead:
                case IdReaderLogicalState.Disconnected:
                    StateCurrentMode = StateMode.Error;
                    StatusCurrentMode = StatusMode.Error;
                    break;
                case IdReaderLogicalState.Uninitialized:
                    StateCurrentMode = StateMode.Uninitialized;
                    break;
                case IdReaderLogicalState.Validating:
                case IdReaderLogicalState.Inspecting:
                    StateCurrentMode = StateMode.Processing;
                    break;
                case IdReaderLogicalState.Reading:
                    StateCurrentMode = StateMode.Processing;
                    StatusCurrentMode = StatusMode.Working;
                    break;
                default:
                    StateCurrentMode = StateMode.Normal;
                    StatusText = string.Empty;
                    break;
            }

            SetIdCardReadData();
        }

        private void SetDeviceInformation()
        {
            var config = IdReader.DeviceConfiguration;

            base.SetDeviceInformation(config);
        }

        /// <summary>Updates status content with given text.</summary>
        /// <param name="text">The text to update.</param>
        /// <param name="removeText">Flag indicating whether to clear text.</param>
        private void UpdateStatusError(string text, bool removeText)
        {
            var temp = StatusText;

            if (removeText)
            {
                if (temp.Contains(text))
                {
                    var temp2 = "\n" + text;
                    var temp3 = text + "\n";
                    if (temp.Contains(temp2))
                    {
                        temp = temp.Replace(temp2, string.Empty);
                    }
                    else if (temp.Contains(temp3))
                    {
                        temp = temp.Replace(temp3, string.Empty);
                    }
                    else
                    {
                        temp = temp.Replace(text, string.Empty);
                    }

                    StatusText = temp;

                    if (!string.IsNullOrEmpty(temp) && !temp.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledByText)))
                    {
                        UpdateStatusErrorMode();
                    }
                    else
                    {
                        StatusCurrentMode = StatusMode.Normal;
                    }
                }
            }
            else
            {
                if (!temp.Contains(text))
                {
                    if (!string.IsNullOrEmpty(temp))
                    {
                        if (temp.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledByText)) ||
                            temp.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IdPresentedText)) ||
                            temp.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IdReaderInspectedText)))
                        {
                            temp = string.Empty;
                        }
                        else
                        {
                            temp += "\n";
                        }
                    }

                    temp += text;

                    StatusText = temp;

                    UpdateStatusErrorMode();
                }
            }
        }

        private void UpdateStatusErrorMode()
        {
            if (StatusCurrentMode != StatusMode.Error)
            {
                var temp = StatusText;

                foreach (var pair in _errorEventText)
                {
                    if (temp.Contains(pair.Value))
                    {
                        StatusCurrentMode = StatusMode.Error;
                    }
                }
            }
        }

        /// <summary>Sets last error status.</summary>
        /// <param name="events">A string of last error events.</param>
        /// <returns>true if last error status set; false otherwise.</returns>
        private bool SetLastErrorStatus(string events)
        {
            if (string.IsNullOrEmpty(events))
            {
                return false;
            }

            foreach (var pair in _errorEventText)
            {
                if (events.Contains(pair.Key))
                {
                    UpdateStatusError(pair.Value, false);
                    events = events.Replace(pair.Key, string.Empty);
                    if (string.IsNullOrEmpty(events))
                    {
                        return true;
                    }
                }
            }

            return true;
        }
    }
}
