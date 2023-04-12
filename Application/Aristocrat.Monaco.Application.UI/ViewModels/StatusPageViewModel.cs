namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.Authentication;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Models;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using MVVM;
    using MVVM.Command;
    using OperatorMenu;
    using NoteAcceptorDisconnectedEvent = Hardware.Contracts.NoteAcceptor.DisconnectedEvent;
    using NoteAcceptorHardwareFaultEvent = Hardware.Contracts.NoteAcceptor.HardwareFaultEvent;
    using PrinterDisconnectedEvent = Hardware.Contracts.Printer.DisconnectedEvent;
    using PrinterHardwareFaultEvent = Hardware.Contracts.Printer.HardwareFaultEvent;
    using ReelControllerHardwareFaultEvent = Hardware.Contracts.Reel.Events.HardwareFaultEvent;
    using ReelControllerReelFaultEvent = Hardware.Contracts.Reel.Events.HardwareReelFaultEvent;

    [CLSCompliant(false)]
    public sealed class StatusPageViewModel : OperatorMenuPageViewModelBase, IMessageDisplayHandler
    {
        // TODO There are more event types that should have info
        private readonly Dictionary<Type, string> _additionalTypeInfos = new Dictionary<Type, string>
        {
            { typeof(OpenEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoCloseDoor) },
            { typeof(ComponentHashCompleteEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoContactTechnician) },
            { typeof(LegitimacyLockUpEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoContactTechnician) },
            { typeof(DisplayDisconnectedEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoScreenDisconnected) },
            { typeof(TouchDisplayDisconnectedEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoTouchScreenDisconnected) },
            { typeof(SystemDisabledEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoOutOfService) },
            { typeof(JamEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoNAJammed) },
            { typeof(NoteAcceptorDisconnectedEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoNADisconnected) },
            { typeof(NoteAcceptorHardwareFaultEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoNAError) },
            { typeof(PrinterDisconnectedEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoPrinterDisconnected) },
            { typeof(PrinterHardwareFaultEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoPrinterError) },
            { typeof(ReelControllerHardwareFaultEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) },
            { typeof(ReelControllerReelFaultEvent), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) }
        };

        private readonly List<Guid> _guidInfosToIgnore = new List<Guid>
        {
            ApplicationConstants.OperatorKeyNotRemovedDisableKey
        };

        private readonly Dictionary<Guid, string> _additionalGuidInfos = new Dictionary<Guid, string>
        {
            { ApplicationConstants.PrinterDisconnectedGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoPrinterDisconnected)},
            { ApplicationConstants.NoteAcceptorDisconnectedGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoNADisconnected)},
            { ApplicationConstants.NoteAcceptorSelfTestFailedGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupNoteAcceptorSelfTestFailedMessage)},
            { ApplicationConstants.ReelControllerDisconnectedGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelControllerDisconnected)},
            { ApplicationConstants.MainDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupMainDoor)},
            { ApplicationConstants.LogicDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupLogicDoor)},
            { ApplicationConstants.LogicSealBrokenKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupLogicSealBroken)},
            { ApplicationConstants.BellyDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupBellyDoor)},
            { ApplicationConstants.CashDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupCashDoor)},
            { ApplicationConstants.SecondaryCashDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupSecondaryCashDoor)},
            { ApplicationConstants.TopBoxDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupTopBoxDoor)},
            { ApplicationConstants.DropDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupDropDoor)},
            { ApplicationConstants.MechanicalMeterDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupMechanicalMeterDoor)},
            { ApplicationConstants.MainOpticDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupMainOpticDoor)},
            { ApplicationConstants.TopBoxOpticDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupTopBoxOpticDoor)},
            { ApplicationConstants.UniversalInterfaceBoxDoorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupUniversalInterfaceBoxDoor)},
            { ApplicationConstants.SystemDisableGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutOfServiceAdditionalInfo)},
            { ApplicationConstants.NoGamesEnabledDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoConfigureGame)},
            { ApplicationConstants.DisplayDisconnectedLockupKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoScreenDisconnected)},
            { ApplicationConstants.TouchDisplayDisconnectedLockupKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoTouchScreenDisconnected)},
            { ApplicationConstants.TouchDisplayReconnectedLockupKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupReconnectedRebootKeyMessage)},
            { ApplicationConstants.LcdButtonDeckDisconnectedLockupKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoPlayerButtonDisconnected)},
            { ApplicationConstants.HandpayPendingDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoHandpayPending) },
            { ApplicationConstants.HostCashOutFailedDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoSasCashoutFailure) },
            { ApplicationConstants.DisabledByHost0Key, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoSasHostDisabled) },
            { ApplicationConstants.DisabledByHost1Key, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoSasHostDisabled) },
            { ApplicationConstants.Host0CommunicationsOfflineDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoSasHostComm) },
            { ApplicationConstants.Host1CommunicationsOfflineDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoSasHostComm) },
            { ApplicationConstants.ValidationIdNeededGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoValidationIdNotSet) },
            { ApplicationConstants.Battery1Guid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoBattery1) },
            { ApplicationConstants.Battery2Guid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoBattery2) },
            { NoteAcceptorFaultTypes.NoteJammed.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoNAJammed) },
            { NoteAcceptorFaultTypes.StackerDisconnected.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoNAStackerRemoved) },
            { NoteAcceptorFaultTypes.StackerFull.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoNAStackerFull) },
            { NoteAcceptorFaultTypes.CheatDetected.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoNAIllegalActivity) },
            { NoteAcceptorFaultTypes.MechanicalFault.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoNAMechanicalError) },
            { NoteAcceptorFaultTypes.FirmwareFault.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupNoteAcceptorFirmwareFaultMessage) },
            { NoteAcceptorFaultTypes.OpticalFault.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupNoteAcceptorHardwareFaultMessage) },
            { NoteAcceptorFaultTypes.ComponentFault.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupNoteAcceptorHardwareFaultMessage) },
            { NoteAcceptorFaultTypes.OtherFault.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupNoteAcceptorUnspecifiedFaultMessage) },
            { PrinterFaultTypes.PaperJam.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoPrinterJammed) },
            { PrinterFaultTypes.PaperEmpty.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoPrinterPaperEmpty) },
            { PrinterFaultTypes.PrintHeadOpen.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoPrinterHeadOpen) },
            { PrinterFaultTypes.TemperatureFault.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupPrinterHardwareFaultMessage) },
            { PrinterFaultTypes.PrintHeadDamaged.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupPrinterHardwareFaultMessage) },
            { PrinterFaultTypes.NvmFault.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupPrinterHardwareFaultMessage) },
            { PrinterFaultTypes.FirmwareFault.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupPrinterHardwareFaultMessage) },
            { PrinterFaultTypes.OtherFault.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupPrinterHardwareFaultMessage) },
            { PrinterFaultTypes.PaperNotTopOfForm.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupPrinterPaperNotTopOfFormFaultMessage) },
            { PrinterFaultTypes.ChassisOpen.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupPrinterChassisOpenFaultMessage) },
            { ReelControllerFaults.CommunicationError.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) },
            { ReelControllerFaults.FirmwareFault.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) },
            { ReelControllerFaults.HardwareError.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) },
            { ReelControllerFaults.LightError.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) },
            { ReelControllerFaults.LowVoltage.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) },
            { ReelControllerFaults.RequestError.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) },
            { ReelFaults.LowVoltage.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) },
            { ReelFaults.ReelStall.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) },
            { ReelFaults.ReelTamper.GetAttribute<ErrorGuidAttribute>().Id, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoReelError) },

            { ApplicationConstants.ExcessiveDocumentRejectGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupExcessiveDocumentReject) },
            { AccountingConstants.DisabledDueToCarrierBoardRemovalKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupDisabledDueToCarrierBoardRemovalKeyMessage) },
            { ApplicationConstants.NoteAcceptorDocumentCheckDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoNADocumentCheck) },
            { ApplicationConstants.ProgressiveUpdateTimeoutGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupProgressiveTimeout) },
            { ApplicationConstants.ProgressiveCommitTimeoutGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearProgressiveCommitTimeout) },
            { ApplicationConstants.ProgressiveDisconnectErrorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearProgressiveDisconnectError) },
            { ApplicationConstants.MinimumThresholdErrorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearMinimumThresholdError) },
            { ApplicationConstants.OperatorResetRequiredDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupOperatorResetMessage).Replace("\\r\\n", Environment.NewLine) },
            { HardwareConstants.AudioDisconnectedLockKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupAudioDisconnectedKeyMessage) },
            { HardwareConstants.AudioReconnectedLockKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupReconnectedRebootKeyMessage) },
            { ApplicationConstants.CurrencyIsoInvalidDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupCurrencyIsoInvalidDisableKeyMessage) },
            { ApplicationConstants.OperatorKeyNotRemovedDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupOperatorKeyNotRemovedDisableKeyMessage) },
            { ApplicationConstants.OperatingHoursDisableGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupOperatingHoursDisableGuidMessage) },
            { ApplicationConstants.OperatorMenuLauncherDisableGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupOperatorMenuLauncherDisableGuidMessage) },
            { ApplicationConstants.FatalGameErrorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupFatalGameErrorGuidMessage) },
            { ApplicationConstants.HardMeterDisabled, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupHardMeterDisabledMessage) },
            { ApplicationConstants.DiskSpaceBelowThresholdDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoContactTechnician) },
            { ApplicationConstants.StorageFaultDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoContactTechnician) },
            { ApplicationConstants.LiveAuthenticationDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoContactTechnician) },
            { ApplicationConstants.EKeyVerifiedDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoEKeyVerified) },
            { ApplicationConstants.SecondaryStorageMediaNotConnectedKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecondaryStorageMediaNotConnectedMessage) },
            { ApplicationConstants.SecondaryStorageMediaConnectedKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecondaryStorageMediaConnectedButNotSupportedMessage) },
            { ApplicationConstants.ReadOnlyMediaDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReadOnlyMediaFaultMessage) },
            { ApplicationConstants.SmartCardRemovedDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SmartCardRemovedFaultMessage) },
            { ApplicationConstants.SmartCardExpiredDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SmartCardExpiredFaultMessage) },
            { ApplicationConstants.SmartCardNotPresentDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SmartCardMissingFaultMessage) },
            { ApplicationConstants.LicenseErrorDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LicenseFileErrorFaultMessage) },
            { ApplicationConstants.ReserveDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReservedMachineErrorFaultMessage) },
            { ApplicationConstants.ExcessiveMeterIncrementErrorGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupExcessiveMeterIncrement) },
            { ApplicationConstants.BellyDoorDiscrepencyGuid, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BellyDoorDiscrepancy) },
            { ApplicationConstants.MemoryBelowThresholdDisableKey, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutOfMemoryMessageDescription) }
        };

        private bool _outOfServiceModeButtonActive;
        private bool _isExitReserveButtonEnabled;

        public StatusPageViewModel()
        {
            InputStatusText = string.Empty;

            // Initially it should always be active until it's set by the rule access service.
            OutOfServiceModeButtonActive = true;

            OutOfServiceModeButtonCommand = new ActionCommand<object>(_ => OutOfServiceModeButtonCommandHandler());

            ExitReserveCommand = new ActionCommand<object>(ExitReserve);
        }

        public ICommand ExitReserveCommand { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether [out of service mode button active].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [out of service mode button active]; otherwise, <c>false</c>.
        /// </value>
        public bool OutOfServiceModeButtonActive
        {
            get => _outOfServiceModeButtonActive;

            set
            {
                _outOfServiceModeButtonActive = value;
                RaisePropertyChanged(nameof(OutOfServiceModeButtonActive));
            }
        }

        /// <summary>
        ///     Gets disable reasons.
        /// </summary>
        public ObservableCollection<StatusMessage> DisableReasons { get; } = new ObservableCollection<StatusMessage>();

        public string OutOfServiceButtonText => DisableByOperatorManager.DisabledByOperator ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnableEGM) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisableEGM);

        /// <summary>
        ///     Gets or sets action command that handles Out of Service Mode button click.
        /// </summary>
        public ICommand OutOfServiceModeButtonCommand { get; set; }

        public bool IsExitReserveButtonVisible
        {
            get => _isExitReserveButtonEnabled;
            private set => SetProperty(ref _isExitReserveButtonEnabled, value, nameof(IsExitReserveButtonVisible));
        }

        private IDisableByOperatorManager DisableByOperatorManager
            => ServiceManager.GetInstance().GetService<IDisableByOperatorManager>();

        private IMessageDisplay MessageDisplay
            => ServiceManager.GetInstance().GetService<IMessageDisplay>();

        public void DisplayMessage(DisplayableMessage displayableMessage)
        {
            // only show hard errors
            if (displayableMessage?.Classification == DisplayableMessageClassification.HardError
                || displayableMessage?.Classification == DisplayableMessageClassification.SoftError)
            {
                Logger.Debug("Displaying messages");

                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        if (_guidInfosToIgnore.Contains(displayableMessage.Id))
                        {
                            Logger.Debug($"Ignoring displayable message ID {displayableMessage.Id}");
                            return;
                        }
                        var additionalInfo = displayableMessage.HelpText;
                        if (string.IsNullOrEmpty(additionalInfo))
                        {
                            if (_additionalGuidInfos.TryGetValue(displayableMessage.Id, out var error))
                            {
                                additionalInfo = error;
                            }
                            else if (displayableMessage.ReasonEvent != null &&
                                _additionalTypeInfos.TryGetValue(displayableMessage.ReasonEvent, out var info))
                            {
                                additionalInfo = info;
                            }
                        }

                        var message = DisableReasons.FirstOrDefault(s => s.Message != null && s.Message.Equals(displayableMessage.Message));

                        if (message != null && string.IsNullOrEmpty(message.AdditionalInfo))
                        {
                            message.AdditionalInfo = additionalInfo;
                        }
                        else
                        {
                            DisableReasons.Add(new StatusMessage(displayableMessage, additionalInfo));
                        }
                    });

                Logger.Debug("Displayed messages");
            }
        }

        public void RemoveMessage(DisplayableMessage displayableMessage)
        {
            if (displayableMessage != null)
            {
                Logger.Debug("Removing messages");

                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        var reason = DisableReasons.LastOrDefault(d => d.Message == displayableMessage.Message);
                        if (reason != null)
                        {
                            DisableReasons.Remove(reason);
                            if (PopupOpen)
                            {
                                EventBus.Publish(new OperatorMenuPopupEvent(false));
                            }
                        }
                    });

                Logger.Debug("Removing messages");
            }
        }

        public void ClearMessages()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    Logger.Debug("Clearing messages");

                    DisableReasons.Clear();

                    Logger.Debug("Cleared messages");
                });
        }

        public void DisplayStatus(string message)
        {
        }

        protected override void OnLoaded()
        {
            MessageDisplay.AddMessageDisplayHandler(this);
            if (DisableByOperatorManager.DisabledByOperator)
            {
                InputStatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutOfServiceReason);
            }

            IsExitReserveButtonVisible = (bool)PropertiesManager.GetProperty(
                ApplicationConstants.ReserveServiceLockupPresent,
                false);

            EventBus?.Subscribe<PropertyChangedEvent>(this, HandleEvent);
        }

        /// <summary>
        ///     Handles unloaded window event.
        /// </summary>
        protected override void OnUnloaded()
        {
            MessageDisplay.RemoveMessageDisplayHandler(this);
            EventBus?.Unsubscribe<PropertyChangedEvent>(this);
        }

        protected override void OnInputStatusChanged()
        {
            bool active = true;
            string text = string.Empty;
            switch (AccessRestriction)
            {
                case OperatorMenuAccessRestriction.InGameRound:
                    text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EndGameRoundBeforeOutOfServiceText);
                    active = false;
                    break;
                case OperatorMenuAccessRestriction.ZeroCredits:
                    // Override the Zero-Credits requirement if this property is set.
                    if (!(bool)PropertiesManager.GetProperty(
                        ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled,
                        false))
                    {
                        text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnteringOutOfServiceModeRequiresZeroCreditsText);
                        active = false;
                    }
                    break;
            }

            OutOfServiceModeButtonActive = active;
            InputStatusText = text;

            if (!active && PopupOpen)
            {
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        EventBus.Publish(new OperatorMenuPopupEvent(false, string.Empty));
                    });
            }
        }

        private void ExitReserve(object obj)
        {
            EventBus?.Publish(new ExitReserveButtonPressedEvent());
        }

        private void OutOfServiceModeButtonCommandHandler()
        {
            if (DisableByOperatorManager.DisabledByOperator)
            {
                DisableByOperatorManager.Enable();
                InputStatusText = string.Empty;
            }
            else
            {
                DisableByOperatorManager.Disable(() => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutOfServiceReason));
                InputStatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutOfServiceReason);
            }

            RaisePropertyChanged(nameof(OutOfServiceButtonText));
        }

        private void HandleEvent(PropertyChangedEvent @event)
        {
            switch (@event.PropertyName)
            {
                //Reserve Machine option is enabled when there's no Machine Reserved lockup already present
                case ApplicationConstants.ReserveServiceLockupPresent:
                    IsExitReserveButtonVisible = (bool)PropertiesManager.GetProperty(
                        ApplicationConstants.ReserveServiceLockupPresent,
                        false);
                    break;
            }
        }
    }
}