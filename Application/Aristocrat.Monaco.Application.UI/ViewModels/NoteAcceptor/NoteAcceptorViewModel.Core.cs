namespace Aristocrat.Monaco.Application.UI.ViewModels.NoteAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Ticket;
    using Helpers;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using MVVM.Command;
    using OperatorMenu;

#pragma warning disable 2214

    /// <summary>
    ///     A NoteAcceptorViewModel contains the logic for NoteAcceptorViewModel.xaml.cs
    /// </summary>
    /// <seealso cref="DeviceViewModel" />
    [CLSCompliant(false)]
    public partial class NoteAcceptorViewModel : DeviceViewModel
    {
        private readonly Dictionary<NoteAcceptorFaultTypes, string> _faultTexts = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorViewModel" /> class.
        /// </summary>
        public NoteAcceptorViewModel(bool isWizard) : base(DeviceType.NoteAcceptor, isWizard)
        {
            Logger.Debug("Start Note Acceptor (MVVM)");

            // Set configurable text strings to coordinate with display message overlay.
            _faultTexts[NoteAcceptorFaultTypes.OtherFault] =
                Resources.NoteAcceptorFaultTypes_OtherFault;
            _faultTexts[NoteAcceptorFaultTypes.StackerJammed] =
                (string)PropertiesManager.GetProperty(
                    ApplicationConstants.NoteAcceptorErrorBillStackerJamText,
                    string.Empty);
            _faultTexts[NoteAcceptorFaultTypes.StackerFault] =
                (string)PropertiesManager.GetProperty(
                    ApplicationConstants.NoteAcceptorErrorBillStackerErrorText,
                    string.Empty);
            _faultTexts[NoteAcceptorFaultTypes.StackerFull] =
                (string)PropertiesManager.GetProperty(
                    ApplicationConstants.NoteAcceptorErrorBillStackerFullText,
                    string.Empty);
            _faultTexts[NoteAcceptorFaultTypes.StackerDisconnected] =
                (string)PropertiesManager.GetProperty(
                    ApplicationConstants.NoteAcceptorErrorCashBoxRemovedText,
                    string.Empty);
            _faultTexts[NoteAcceptorFaultTypes.NoteJammed] =
                (string)PropertiesManager.GetProperty(
                    ApplicationConstants.NoteAcceptorErrorBillJamText,
                    string.Empty);

            var noteAcceptor = NoteAcceptor;

            CanEditBillAcceptanceLimit = GetConfigSetting(OperatorMenuSetting.EgmCanEditBillAcceptorLimit, true);
            HideBillAcceptorLimitField = GetConfigSetting(OperatorMenuSetting.HideBillAcceptorLimitField, false);

            Denominations = new List<ConfigurableDenomination>();
            if (noteAcceptor != null)
            {
                foreach (var denom in noteAcceptor.GetSupportedNotes())
                {
                    Denominations.Add(new ConfigurableDenomination(
                        denom,
                        new ActionCommand<bool>(b => HandleDenominationChangeCommand(b, denom)),
                        DenominationIsSelected(denom)));
                }
            }

            InitCommands();

            _isDenomEditable = GetConfigSetting(OperatorMenuSetting.EgmCanEnableDenominations, false);

            var excessiveDocumentRejectCount = PropertiesManager.GetValue(ApplicationConstants.ExcessiveDocumentRejectCount, -1);
            var excessiveDocumentRejectCountDefault = PropertiesManager.GetValue(ApplicationConstants.ExcessiveDocumentRejectCountDefault, -1);

            _excessiveRejectDisable = excessiveDocumentRejectCount > -1;
            _excessiveRejectDisableIsVisible = excessiveDocumentRejectCountDefault > -1;

            _allowBillIn = IsAnyDenomsSelected();
        }

        protected override void UpdateScreen()
        {
            lock (EventLock)
            {
                var noteAcceptor = NoteAcceptor;
                if (EventHandlerStopped || noteAcceptor == null)
                {
                    return;
                }

                if (_noteAcceptorDiagnosticsEnabled &&
                    noteAcceptor.LogicalState != NoteAcceptorLogicalState.Uninitialized &&
                    !string.IsNullOrEmpty(noteAcceptor.DeviceConfiguration.Protocol))
                {
                    SetDiagnosticButtonsVisible(true);
                }
                else
                {
                    SetDiagnosticButtonsVisible(false);
                }

                IsVoucherInEnabled = PropertiesManager.GetValue(PropertyKey.VoucherIn, false);

                VoucherInEnabledText = IsVoucherInEnabled && noteAcceptor.Enabled ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledLabel) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disabled);

                RaisePropertyChanged(nameof(TestModeEnabled));

                ConfigureStackButton();

                ConfigureViewForUninitializedState();

                SetStateInformation(false);

                LastDocumentResultText = noteAcceptor.LastDocumentResult.ToString();

                StackerStateText = noteAcceptor.StackerState.ToString();
            }
        }

        private void SetActivationDateTime()
        {
            var visibility = GetConfigSetting(OperatorMenuSetting.ActivationTimeVisible, false);

            if (visibility)
            {
                ActivationVisible = true;
                var activationTime = NoteAcceptor?.ActivationTime ?? DateTime.MinValue;
                var time = ServiceManager.GetInstance().GetService<ITime>();
                ActivationTime = activationTime == DateTime.MinValue
                    ? string.Empty
                    : time.GetLocationTime(activationTime).ToString();
            }
            else
            {
                ActivationVisible = false;
            }
        }

        private void ConfigureStackButton()
        {
            if (_noteAcceptorDiagnosticsEnabled && NoteAcceptor.IsEscrowed && !_inNoteAcceptorTest)
            {
                ReturnButtonVisible = true;
                ReturnButtonEnabled = true;
            }
            else
            {
                ReturnButtonVisible = false;
                StackButtonVisible = false;
            }
        }

        private void ConfigureViewForUninitializedState()
        {
            // Is the note acceptor uninitialized we are not displaying the GAT report?
            if (NoteAcceptor.LogicalState == NoteAcceptorLogicalState.Uninitialized)
            {
                // Yes, set device information unknown.
                SetDeviceInformationUnknown();

                // Set Inspect button and COM port combo box visibility based on whether or not we ar currently inspecting.
                InspectButtonVisible = !_inspecting;
            }
            else
            {
                // Hide Inspect button and COM port combo box.
                InspectButtonVisible = false;
            }
        }

        private void ConfigureDenominations()
        {
            if (NoteAcceptor == null ||
                !NoteAcceptor.Connected ||
                NoteAcceptor.LogicalState == NoteAcceptorLogicalState.Uninitialized)
            {
                Logger.Error("Note Acceptor Denoms not ready");
                return;
            }

            var selectedDenoms = new StringBuilder();

            // Set checkboxes enabled based on available denominations
            foreach (var denom in Denominations)
            {
                selectedDenoms.Append(
                    ConfigureDenomination(
                        denom.Denom,
                        out var selected,
                        out var enabled));
                denom.Selected = selected;
                denom.Enabled = enabled;
            }

            VoucherInEnabledText = IsVoucherInEnabled && NoteAcceptor.Enabled ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledLabel) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disabled);
            _enabledDenominationsText = selectedDenoms.ToString();
        }

        /// <summary>
        ///     Configure denomination selected and enabled for note acceptor
        /// </summary>
        /// <returns>Denomination if selected.</returns>
        public string ConfigureDenomination(int denomination, out bool selected, out bool enabled)
        {
            if (NoteAcceptor == null)
            {
                selected = enabled = false;
                return string.Empty;
            }

            var selectedDenominations = NoteAcceptor.Denominations;
            var availableDenominations = NoteAcceptor.GetSupportedNotes();

            if (availableDenominations.Count == 0)
            {
                selected = false;
                enabled = false;
            }
            else
            {
                selected = selectedDenominations.Contains(denomination) && !NoteAcceptor.IsNoteDisabled(denomination);
                enabled = selectedDenominations.Count == availableDenominations.Count;
            }

            if (selected)
            {
                return denomination + " ";
            }

            return string.Empty;
        }

        public bool DenominationIsSelected(int denomination)
        {
            if (NoteAcceptor == null)
            {
                return false;
            }

            var selectedDenominations = NoteAcceptor.Denominations;
            var availableDenominations = NoteAcceptor.GetSupportedNotes();

            return availableDenominations.Contains(denomination) && selectedDenominations.Contains(denomination);
        }

        private void StartInspecting()
        {
            _inspecting = true;

            Logger.DebugFormat(
                "Inspecting for {0} note acceptor on {1}",
                NoteAcceptor.DeviceConfiguration.Protocol,
                NoteAcceptor.DeviceConfiguration.PortName);
            Task.Run(() => NoteAcceptor.Inspect(InspectionTimeout));
        }

        private void UpdateView()
        {
            if (NoteAcceptor != null)
            {
                InitializeNoteAcceptorDisplayInformation();
            }
            else
            {
                SetStateInformation(true);
                StackerStateText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                VoucherInEnabledText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                LastDocumentResultText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                Logger.Warn("NoteAcceptorService and DfuService unavailable");
            }
        }

        private void InitializeNoteAcceptorDisplayInformation()
        {
            BillAcceptanceLimit = _initialBillAcceptanceLimit = PropertiesManager.GetValue(PropertyKey.MaxCreditsIn, ApplicationConstants.DefaultMaxCreditsIn).MillicentsToDollars();
            BillAcceptanceLimitIsChecked = BillAcceptanceLimit < ApplicationConstants.DefaultMaxCreditsIn.MillicentsToDollars();

            SetStateInformation(true);

            if (NoteAcceptor == null)
            {
                return;
            }

            LastDocumentResultText = NoteAcceptor.LastDocumentResult.ToString();

            StackerStateText = NoteAcceptor.StackerState.ToString();
            SetDenominationsVisible(IsDenominationsVisible);
            ConfigureDenominations();

            if (NoteAcceptor.LogicalState != NoteAcceptorLogicalState.Uninitialized)
            {
                if (!NoteAcceptor.Enabled)
                {
                    Logger.InfoFormat("Disabled reason: {0}", NoteAcceptor.ReasonDisabled);
                    if (_noteAcceptorDiagnosticsEnabled)
                    {
                        SetDiagnosticButtonsEnabled(true);
                        if (NoteAcceptor.LogicalState != NoteAcceptorLogicalState.Uninitialized &&
                            !string.IsNullOrEmpty(NoteAcceptor.DeviceConfiguration.Protocol))
                        {
                            SetDiagnosticButtonsVisible(true);
                        }
                    }

                    if (string.IsNullOrEmpty(StatusText) || StatusText.Equals(StatusMode.Error.ToString()) || HasDocumentCheckFault)
                    {
                        if (!NoteAcceptor.Enabled || !NoteAcceptor.Connected)
                        {
                            if (NoteAcceptor.LastError.Contains(typeof(SelfTestFailedEvent).ToString()) || HasDocumentCheckFault)
                            {
                                SelfTestCurrentState = SelfTestState.Failed;
                                SetDiagnosticButtonsEnabled(true);
                            }
                            else if (IsEnableAllowedForTesting())
                            {
                                SetDiagnosticButtonsEnabled(true);
                            }
                            else
                            {
                                SetDiagnosticButtonsEnabled(false);
                            }
                        }
                        else
                        {
                            SetDiagnosticButtonsEnabled(true);
                        }
                    }
                }
                else
                {
                    SetDiagnosticButtonsEnabled(true);
                }

                SetDeviceInformation();
            }
            else
            {
                SetDeviceInformationUnknown();
            }

            // Update connection status
            if (NoteAcceptor.Connected)
            {
                RemoveStatusMessage(DisconnectedEvent.NoteAcceptorDisconnectedText);
            }
            else
            {
                AddStatusMessage(DisconnectedEvent.NoteAcceptorDisconnectedText, StatusMode.Error);
            }

            UpdateStatus();
        }

        private IEnumerable<string> GetFaultDescriptions()
        {
            var faults = new List<string>();
            if (NoteAcceptor == null)
            {
                return faults;
            }

            foreach (NoteAcceptorFaultTypes fault in Enum.GetValues(typeof(NoteAcceptorFaultTypes)))
            {
                if (_faultTexts.ContainsKey(fault) && NoteAcceptor.Faults.HasFlag(fault))
                {
                    faults.Add(_faultTexts[fault]);
                }
            }

            if (HasDocumentCheckFault)
            {
                faults.Add(Localizer
                    .For(CultureFor.Operator)
                    .GetString(ResourceKeys.NoteAcceptorFaultTypes_DocumentCheck));
            }

            return faults;
        }

        private void SetDiagnosticButtonsEnabled(bool enable)
        {
            if (NoteAcceptor != null)
            {
                var canEnableSelfTest = false;
                if (!NoteAcceptor.Enabled)
                {
                    if ((NoteAcceptor.ReasonDisabled & DisabledReasons.Error) > 0)
                    {
                        canEnableSelfTest = SelfTestCurrentState != SelfTestState.Running;
                    }
                    else if (IsEnableAllowedForTesting(HasDocumentCheckFault))
                    {
                        canEnableSelfTest = true;
                    }
                }
                else
                {
                    canEnableSelfTest = true;
                }

                SelfTestButtonEnabled = enable &&
                                        canEnableSelfTest &&
                                        NoteAcceptor.Connected &&
                                        NoteAcceptor.StackerState != NoteAcceptorStackerState.Removed;
            }
            else
            {
                SelfTestButtonEnabled = false;
            }

            ReturnButtonEnabled = enable;
            SetDenominationsEnabled(enable);
            RaisePropertyChanged(nameof(TestModeEnabled));
        }

        private void SetDenominationsEnabled(bool enable)
        {
            foreach (var denom in Denominations)
            {
                denom.Enabled = enable;
            }

            AllowBillInEnabled = enable;
        }

        private void SetDiagnosticButtonsVisible(bool visible)
        {
            SelfTestButtonVisible = visible;
            SelfTestClearNvmButtonVisible = visible;
        }

        private void SetDenominationsVisible(bool visible)
        {
            foreach (var denom in Denominations)
            {
                denom.Visible = visible;
            }
        }

        private void SetStateInformation(bool updateStatus)
        {
            if (_inspecting)
            {
                StateText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Inspecting);
                StateCurrentMode = StateMode.Processing;

                return;
            }

            var logicalState = NoteAcceptor?.LogicalState ?? NoteAcceptorLogicalState.Disabled;

            StateText = logicalState.ToString();

            switch (logicalState)
            {
                case NoteAcceptorLogicalState.Disabled:
                    StateCurrentMode = StateMode.Error;
                    break;
                case NoteAcceptorLogicalState.Disconnected:
                    StateCurrentMode = StateMode.Error;
                    break;
                case NoteAcceptorLogicalState.Uninitialized:
                    StateCurrentMode = StateMode.Uninitialized;
                    break;
                case NoteAcceptorLogicalState.InEscrow:
                    StateCurrentMode = StateMode.Warning;
                    break;
                case NoteAcceptorLogicalState.Inspecting:
                case NoteAcceptorLogicalState.Returning:
                case NoteAcceptorLogicalState.Stacking:
                    StateCurrentMode = StateMode.Processing;
                    break;
                default:
                    if (!HasDocumentCheckFault &&
                        (StateCurrentMode != StateMode.Normal || StatusCurrentMode != StatusMode.None))
                    {
                        StateCurrentMode = StateMode.Normal;
                        updateStatus = true;
                    }
                    break;
            }

            if (updateStatus)
            {
                if (!NoteAcceptor?.Connected ?? false)
                {
                    HandleEvent(new DisconnectedEvent());
                }
                else
                {
                    UpdateStatus();
                }
            }
        }

        private StatusMode GetHighestStatusMode(StatusMode mode1, StatusMode mode2)
        {
            if (mode1 == StatusMode.None)
            {
                return mode2;
            }

            if (mode2 == StatusMode.None)
            {
                return mode1;
            }

            return mode1 > mode2 ? mode1 : mode2;
        }

        private void AddStatusMessage(string message, StatusMode mode)
        {
            if (!_statusMessages.ContainsKey(message))
            {
                Logger.DebugFormat("ADD {0}", message);
                _statusMessages.Add(message, mode);
            }

            UpdateStatus();
        }

        private void RemoveStatusMessage(string message)
        {
            Logger.DebugFormat("CLEAR {0}", message);
            _statusMessages.Remove(message);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (NoteAcceptor == null)
            {
                Logger.Warn("Note acceptor service unavailable");
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                StatusCurrentMode = StatusMode.None;
                return;
            }

            if (EventHandlerStopped)
            {
                return;
            }

            var faults = GetFaultDescriptions().ToList();
            if (faults.Any() || _statusMessages.Any())
            {
                var status = string.Empty;
                var highestStatusMode = StatusMode.None;
                foreach (var fault in faults)
                {
                    Inspection?.SetTestName($"hardware fault {fault}");
                    Inspection?.ReportTestFailure();
                    status += fault + Environment.NewLine;
                    highestStatusMode = StatusMode.Error;
                }

                foreach (var message in _statusMessages)
                {
                    status += message.Key + Environment.NewLine;
                    highestStatusMode = GetHighestStatusMode(highestStatusMode, message.Value);
                }

                StatusText = status;
                StatusCurrentMode = highestStatusMode;

                return;
            }

            if (!NoteAcceptor.Connected)
            {
                var disconnected = new DisconnectedEvent();
                StatusText = disconnected.ToString();
                StatusCurrentMode = StatusMode.Error;

                return;
            }

            if ((NoteAcceptor.ReasonDisabled & DisabledReasons.Error) > 0)
            {
                StatusText = NoteAcceptor.LastError;

                if (NoteAcceptor.LastError.Contains(typeof(SelfTestFailedEvent).ToString()))
                {
                    StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SelfTestFailed);
                }

                StatusCurrentMode = StatusMode.Warning;

                return;
            }

            if ((NoteAcceptor.ReasonDisabled & DisabledReasons.GamePlay) > 0 && !GameIdle)
            {
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Game);
                StatusCurrentMode = StatusMode.Warning;

                return;
            }

            if ((NoteAcceptor.ReasonDisabled & DisabledReasons.Backend) > 0)
            {
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Backend);
                StatusCurrentMode = StatusMode.Warning;

                return;
            }

            StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoErrors);
            StatusCurrentMode = StatusMode.None;
        }

        private void UpdateStatus(string overrideMessage, StatusMode overrideStatusMode = StatusMode.Normal)
        {
            if (!string.IsNullOrEmpty(overrideMessage))
            {
                StatusText = overrideMessage;
                StatusCurrentMode = overrideStatusMode;

                return;
            }

            SetStateInformation(true);
        }

        protected void SetDeviceInformation()
        {
            if (NoteAcceptor == null)
            {
                return;
            }

            SetDeviceInformation(NoteAcceptor.DeviceConfiguration);

            VariantNameText = NoteAcceptor.DeviceConfiguration.VariantName;
            VariantVersionText = NoteAcceptor.DeviceConfiguration.VariantVersion;
            SetActivationDateTime();
        }

        internal override void SetDeviceInformationUnknown()
        {
            base.SetDeviceInformationUnknown();

            VariantNameText = "?";
            VariantVersionText = "?";
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            var tempString = CreateNoteAcceptorReportInfo();
            var testTemp = string.Empty;
            ReportComponentInformation(ref testTemp);

            tempString += testTemp;

            return CreateInformationTicket(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoteAcceptorLabel), tempString);
        }

        private string CreateNoteAcceptorReportInfo()
        {
            var tempString = new StringBuilder();

            tempString.Append(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ManufacturerLabel) + ": ");
            tempString.Append(ManufacturerText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ModelLabel) + ": ");
            tempString.Append(ModelText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProtocolLabel) + ": ");
            tempString.Append(ProtocolText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Port) + ": ");
            tempString.Append(PortText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SerialNumberLabel) + ": ");
            tempString.Append(SerialNumberText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FirmwareVersionLabel) + ": ");
            tempString.Append(FirmwareVersionText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FirmwareRevisionLabel) + ": ");
            tempString.Append(FirmwareRevisionText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FirmwareCRCLabel) + ": ");
            tempString.Append(FirmwareCrcText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VariantNameLabel) + ": ");
            tempString.Append(string.IsNullOrEmpty(VariantNameText) ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable) : VariantNameText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VariantVersionLabel) + ": ");
            tempString.Append(string.IsNullOrEmpty(VariantVersionText) ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable) : VariantVersionText);

            tempString.Append("\n \n");
            tempString.Append(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StateLabel) + ": ");
            tempString.Append(StateText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LastDocumentResultLabel) + ": ");
            tempString.Append(LastDocumentResultText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StackerLabel) + ": ");
            tempString.Append(StackerStateText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SelfTestLabel) + ": ");
            tempString.Append(SelfTestText);
            if (ActivationVisible)
                tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ActivationTimeLabel) + ": ");
            tempString.Append(ActivationTime);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StatusLabel) + ": ");
            tempString.Append(StatusText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherIn) + ": ");
            tempString.Append(VoucherInEnabledText);
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BillAcceptanceLimit) + ": ");
            tempString.Append(
                BillAcceptanceLimit > 0
                    ? BillAcceptanceLimit.FormattedCurrencyString()
                    : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit));
            tempString.Append("\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledDenominationsLabel) + ": ");
            if (!string.IsNullOrEmpty(EnabledDenominationsText))
            {
                var enabledDenominations =
                    EnabledDenominationsText.GetDigitalFormatOfDenominations(NoteAcceptor.GetSupportedNotes());
                const int maxEnabledDenominationsLineLen = 15;
                if (enabledDenominations.Length < maxEnabledDenominationsLineLen)
                {
                    tempString.Append(enabledDenominations);
                }
                else
                {
                    var tempArray = enabledDenominations.Split(' ');
                    var temp = new StringBuilder();

                    foreach (var item in tempArray)
                    {
                        if (temp.Length + item.Length >= maxEnabledDenominationsLineLen)
                        {
                            temp.Append("\n");
                            tempString.Append(temp);
                            temp.Clear();
                        }

                        temp.Append(item + ' ');
                    }

                    tempString.Append(temp);
                }
            }

            tempString.Append("\n \n");

            return tempString.ToString();
        }

        /// <summary>Reports component information.</summary>
        /// <param name="reportData">A string reference that will contain the Component Information.</param>
        private void ReportComponentInformation(ref string reportData)
        {
            if (NoteAcceptor == null)
            {
                Logger.Warn("Note acceptor service unavailable");
                return;
            }

            var currentDir = Directory.GetCurrentDirectory();

            var componentFiles = Directory.GetFiles(currentDir, "NoteAcceptor*.dll");
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
                else if (NoteAcceptor.LogicalState != NoteAcceptorLogicalState.Uninitialized)
                {
                    var device = NoteAcceptor.DeviceConfiguration;
                    if (assemblyName.Name.Contains(device.Protocol))
                    {
                        reportData += assemblyName.Name + " " + assemblyName.Version;
                        reportData += "\n";
                    }
                }
            }

            var wpfFiles = Directory.GetFiles(currentDir, "WpfNoteAcceptor*.dll");

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

        protected override void OnInputStatusChanged()
        {
            if (!InputEnabled)
            {
                ChangeFocus = true;
                CloseTouchScreenKeyboard();
                ChangeFocus = false;
            }
        }

        protected override void OnInputEnabledChanged()
        {
            RaisePropertyChanged(nameof(CanEgmModifyDenominations));
        }

        /// <summary>This method will check whether any of the enabled denominations are currently selected or not</summary>
        /// <returns>true if even a single enabled denoms is selected, otherwise false</returns>
        private bool IsAnyDenomsSelected()
        {
            return Denominations.Any(denom => denom.Enabled && denom.Selected);
        }

        private bool IsEnableAllowedForTesting(bool allowDuringGameRound = false)
        {
            if (NoteAcceptor is not { Connected: true })
            {
                return false;
            }

            return NoteAcceptor.ReasonDisabled > 0 && (GameIdle || allowDuringGameRound) &&
                   (NoteAcceptor.ReasonDisabled |
                    DisabledReasons.System |
                    DisabledReasons.Backend |
                    DisabledReasons.Device |
                    DisabledReasons.Configuration |
                    DisabledReasons.GamePlay) ==
                   (DisabledReasons.System |
                    DisabledReasons.Backend |
                    DisabledReasons.Device |
                    DisabledReasons.Configuration |
                    DisabledReasons.GamePlay);
        }
    }

#pragma warning restore 2214
}