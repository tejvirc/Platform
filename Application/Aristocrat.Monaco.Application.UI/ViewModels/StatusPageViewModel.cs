namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Accounting.Contracts;
    using Aristocrat.Extensions.CommunityToolkit;
    using CommunityToolkit.Mvvm.Input;
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
        private readonly Dictionary<Type, string> _additionalTypeInfoKeys = new Dictionary<Type, string>
        {
            { typeof(OpenEvent), ResourceKeys.ErrorInfoCloseDoor },
            { typeof(ComponentHashCompleteEvent), ResourceKeys.ErrorInfoContactTechnician },
            { typeof(LegitimacyLockUpEvent), ResourceKeys.ErrorInfoContactTechnician },
            { typeof(DisplayDisconnectedEvent), ResourceKeys.ErrorInfoScreenDisconnected },
            { typeof(TouchDisplayDisconnectedEvent), ResourceKeys.ErrorInfoTouchScreenDisconnected },
            { typeof(SystemDisabledEvent), ResourceKeys.ErrorInfoOutOfService },
            { typeof(JamEvent), ResourceKeys.ErrorInfoNAJammed },
            { typeof(NoteAcceptorDisconnectedEvent), ResourceKeys.ErrorInfoNADisconnected },
            { typeof(NoteAcceptorHardwareFaultEvent), ResourceKeys.ErrorInfoNAError },
            { typeof(PrinterDisconnectedEvent), ResourceKeys.ErrorInfoPrinterDisconnected },
            { typeof(PrinterHardwareFaultEvent), ResourceKeys.ErrorInfoPrinterError },
            { typeof(ReelControllerHardwareFaultEvent), ResourceKeys.ErrorInfoReelError },
            { typeof(ReelControllerReelFaultEvent), ResourceKeys.ErrorInfoReelError }
        };

        private readonly List<Guid> _guidInfosToIgnore = new List<Guid>
        {
            ApplicationConstants.OperatorKeyNotRemovedDisableKey
        };

        private readonly Dictionary<Guid, string> _additionalGuidInfoKeys = new Dictionary<Guid, string>
        {
            { ApplicationConstants.PrinterDisconnectedGuid, ResourceKeys.ErrorInfoPrinterDisconnected },
            { ApplicationConstants.NoteAcceptorDisconnectedGuid, ResourceKeys.ErrorInfoNADisconnected },
            { ApplicationConstants.NoteAcceptorSelfTestFailedGuid, ResourceKeys.ClearLockupNoteAcceptorSelfTestFailedMessage },
            { ApplicationConstants.ReelControllerDisconnectedGuid, ResourceKeys.ErrorInfoReelControllerDisconnected },
            { ApplicationConstants.MainDoorGuid, ResourceKeys.ClearLockupMainDoor },
            { ApplicationConstants.LogicDoorGuid, ResourceKeys.ClearLockupLogicDoor },
            { ApplicationConstants.LogicSealBrokenKey, ResourceKeys.ClearLockupLogicSealBroken },
            { ApplicationConstants.BellyDoorGuid, ResourceKeys.ClearLockupBellyDoor },
            { ApplicationConstants.CashDoorGuid, ResourceKeys.ClearLockupCashDoor },
            { ApplicationConstants.SecondaryCashDoorGuid, ResourceKeys.ClearLockupSecondaryCashDoor },
            { ApplicationConstants.TopBoxDoorGuid, ResourceKeys.ClearLockupTopBoxDoor },
            { ApplicationConstants.DropDoorGuid, ResourceKeys.ClearLockupDropDoor },
            { ApplicationConstants.MechanicalMeterDoorGuid, ResourceKeys.ClearLockupMechanicalMeterDoor },
            { ApplicationConstants.MainOpticDoorGuid, ResourceKeys.ClearLockupMainOpticDoor },
            { ApplicationConstants.TopBoxOpticDoorGuid, ResourceKeys.ClearLockupTopBoxOpticDoor },
            { ApplicationConstants.UniversalInterfaceBoxDoorGuid, ResourceKeys.ClearLockupUniversalInterfaceBoxDoor },
            { ApplicationConstants.SystemDisableGuid, ResourceKeys.OutOfServiceAdditionalInfo },
            { ApplicationConstants.NoGamesEnabledDisableKey, ResourceKeys.ErrorInfoConfigureGame },
            { ApplicationConstants.DisplayDisconnectedLockupKey, ResourceKeys.ErrorInfoScreenDisconnected },
            { ApplicationConstants.TouchDisplayDisconnectedLockupKey, ResourceKeys.ErrorInfoTouchScreenDisconnected },
            { ApplicationConstants.TouchDisplayReconnectedLockupKey, ResourceKeys.ClearLockupReconnectedRebootKeyMessage },
            { ApplicationConstants.LcdButtonDeckDisconnectedLockupKey, ResourceKeys.ErrorInfoPlayerButtonDisconnected },
            { ApplicationConstants.HandpayPendingDisableKey, ResourceKeys.ErrorInfoHandpayPending },
            { ApplicationConstants.HostCashOutFailedDisableKey, ResourceKeys.ErrorInfoSasCashoutFailure },
            { ApplicationConstants.DisabledByHost0Key, ResourceKeys.ErrorInfoSasHostDisabled },
            { ApplicationConstants.DisabledByHost1Key, ResourceKeys.ErrorInfoSasHostDisabled },
            { ApplicationConstants.Host0CommunicationsOfflineDisableKey, ResourceKeys.ErrorInfoSasHostComm },
            { ApplicationConstants.Host1CommunicationsOfflineDisableKey, ResourceKeys.ErrorInfoSasHostComm },
            { ApplicationConstants.ValidationIdNeededGuid, ResourceKeys.ErrorInfoValidationIdNotSet },
            { ApplicationConstants.Battery1Guid, ResourceKeys.ErrorInfoBattery1 },
            { ApplicationConstants.Battery2Guid, ResourceKeys.ErrorInfoBattery2 },
            { NoteAcceptorFaultTypes.NoteJammed.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoNAJammed },
            { NoteAcceptorFaultTypes.StackerDisconnected.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoNAStackerRemoved },
            { NoteAcceptorFaultTypes.StackerFull.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoNAStackerFull },
            { NoteAcceptorFaultTypes.CheatDetected.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoNAIllegalActivity },
            { NoteAcceptorFaultTypes.MechanicalFault.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoNAMechanicalError },
            { NoteAcceptorFaultTypes.FirmwareFault.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupNoteAcceptorFirmwareFaultMessage },
            { NoteAcceptorFaultTypes.OpticalFault.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupNoteAcceptorHardwareFaultMessage },
            { NoteAcceptorFaultTypes.ComponentFault.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupNoteAcceptorHardwareFaultMessage },
            { NoteAcceptorFaultTypes.OtherFault.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupNoteAcceptorUnspecifiedFaultMessage },
            { PrinterFaultTypes.PaperJam.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoPrinterJammed },
            { PrinterFaultTypes.PaperEmpty.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoPrinterPaperEmpty },
            { PrinterFaultTypes.PrintHeadOpen.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoPrinterHeadOpen },
            { PrinterFaultTypes.TemperatureFault.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupPrinterHardwareFaultMessage },
            { PrinterFaultTypes.PrintHeadDamaged.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupPrinterHardwareFaultMessage },
            { PrinterFaultTypes.NvmFault.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupPrinterHardwareFaultMessage },
            { PrinterFaultTypes.FirmwareFault.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupPrinterHardwareFaultMessage },
            { PrinterFaultTypes.OtherFault.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupPrinterHardwareFaultMessage },
            { PrinterFaultTypes.PaperNotTopOfForm.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupPrinterPaperNotTopOfFormFaultMessage },
            { PrinterFaultTypes.ChassisOpen.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ClearLockupPrinterChassisOpenFaultMessage },
            { PrinterWarningTypes.PaperLow.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoPrinterPaperLow },
            { ReelControllerFaults.CommunicationError.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoReelError },
            { ReelControllerFaults.FirmwareFault.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoReelError },
            { ReelControllerFaults.HardwareError.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoReelError },
            { ReelControllerFaults.LightError.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoReelError },
            { ReelControllerFaults.LowVoltage.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoReelError },
            { ReelControllerFaults.RequestError.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoReelError },
            { ReelFaults.LowVoltage.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoReelError },
            { ReelFaults.ReelStall.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoReelError },
            { ReelFaults.ReelOpticSequenceError.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoReelError },
            { ReelFaults.ReelTamper.GetAttribute<ErrorGuidAttribute>().Id, ResourceKeys.ErrorInfoReelError },

            { ApplicationConstants.ExcessiveDocumentRejectGuid, ResourceKeys.ClearLockupExcessiveDocumentReject },
            { AccountingConstants.DisabledDueToCarrierBoardRemovalKey, ResourceKeys.ClearLockupDisabledDueToCarrierBoardRemovalKeyMessage },
            { ApplicationConstants.NoteAcceptorDocumentCheckDisableKey, ResourceKeys.ErrorInfoNADocumentCheck },
            { ApplicationConstants.ProgressiveUpdateTimeoutGuid, ResourceKeys.ClearLockupProgressiveTimeout },
            { ApplicationConstants.ProgressiveCommitTimeoutGuid, ResourceKeys.ClearProgressiveCommitTimeout },
            { ApplicationConstants.ProgressiveDisconnectErrorGuid, ResourceKeys.ClearProgressiveDisconnectError },
            { ApplicationConstants.MinimumThresholdErrorGuid, ResourceKeys.ClearMinimumThresholdError },
            { ApplicationConstants.OperatorResetRequiredDisableKey, ResourceKeys.ClearLockupOperatorResetMessage },
            { HardwareConstants.AudioDisconnectedLockKey, ResourceKeys.ClearLockupAudioDisconnectedKeyMessage },
            { HardwareConstants.AudioReconnectedLockKey, ResourceKeys.ClearLockupReconnectedRebootKeyMessage },
            { ApplicationConstants.CurrencyIsoInvalidDisableKey, ResourceKeys.ClearLockupCurrencyIsoInvalidDisableKeyMessage },
            { ApplicationConstants.OperatorKeyNotRemovedDisableKey, ResourceKeys.ClearLockupOperatorKeyNotRemovedDisableKeyMessage },
            { ApplicationConstants.OperatingHoursDisableGuid, ResourceKeys.ClearLockupOperatingHoursDisableGuidMessage },
            { ApplicationConstants.OperatorMenuLauncherDisableGuid, ResourceKeys.ClearLockupOperatorMenuLauncherDisableGuidMessage },
            { ApplicationConstants.FatalGameErrorGuid, ResourceKeys.ClearLockupFatalGameErrorGuidMessage },
            { ApplicationConstants.HardMeterDisabled, ResourceKeys.ClearLockupHardMeterDisabledMessage },
            { ApplicationConstants.DiskSpaceBelowThresholdDisableKey, ResourceKeys.ErrorInfoContactTechnician },
            { ApplicationConstants.StorageFaultDisableKey, ResourceKeys.ErrorInfoContactTechnician },
            { ApplicationConstants.LiveAuthenticationDisableKey, ResourceKeys.ErrorInfoContactTechnician },
            { ApplicationConstants.EKeyVerifiedDisableKey, ResourceKeys.ErrorInfoEKeyVerified },
            { ApplicationConstants.SecondaryStorageMediaNotConnectedKey, ResourceKeys.SecondaryStorageMediaNotConnectedMessage },
            { ApplicationConstants.SecondaryStorageMediaConnectedKey, ResourceKeys.SecondaryStorageMediaConnectedButNotSupportedMessage },
            { ApplicationConstants.ReadOnlyMediaDisableKey, ResourceKeys.ReadOnlyMediaFaultMessage },
            { ApplicationConstants.SmartCardRemovedDisableKey, ResourceKeys.SmartCardRemovedFaultMessage },
            { ApplicationConstants.SmartCardExpiredDisableKey, ResourceKeys.SmartCardExpiredFaultMessage },
            { ApplicationConstants.SmartCardNotPresentDisableKey, ResourceKeys.SmartCardMissingFaultMessage },
            { ApplicationConstants.LicenseErrorDisableKey, ResourceKeys.LicenseFileErrorFaultMessage },
            { ApplicationConstants.ReserveDisableKey, ResourceKeys.ReservedMachineErrorFaultMessage },
            { ApplicationConstants.ExcessiveMeterIncrementErrorGuid, ResourceKeys.ClearLockupExcessiveMeterIncrement },
            { ApplicationConstants.BellyDoorDiscrepencyGuid, ResourceKeys.BellyDoorDiscrepancy },
            { ApplicationConstants.MemoryBelowThresholdDisableKey, ResourceKeys.OutOfMemoryMessageDescription },
            { ApplicationConstants.ReelLoadingAnimationFilesDisableKey, ResourceKeys.LoadingAnimationFilesInfo },
            { ApplicationConstants.ReelLoadingAnimationFilesErrorKey, ResourceKeys.ClearLockupReconnectedRebootKeyMessage }
        };

        private bool _isExitReserveButtonEnabled;

        public StatusPageViewModel()
        {
            InputStatusText = string.Empty;

            ExitReserveCommand = new RelayCommand<object>(ExitReserve);

            OutOfServiceViewModel = new OutOfServiceViewModel();
        }

        public ICommand ExitReserveCommand { get; }

        /// <summary>
        ///     Gets disable reasons.
        /// </summary>
        public ObservableCollection<StatusMessage> DisableReasons { get; } = new ObservableCollection<StatusMessage>();

        public bool IsExitReserveButtonVisible
        {
            get => _isExitReserveButtonEnabled;
            private set => SetProperty(ref _isExitReserveButtonEnabled, value);
        }

        private IDisableByOperatorManager DisableByOperatorManager
            => ServiceManager.GetInstance().GetService<IDisableByOperatorManager>();

        public OutOfServiceViewModel OutOfServiceViewModel { get; }

        private IMessageDisplay MessageDisplay => ServiceManager.GetInstance().GetService<IMessageDisplay>();

        public void DisplayMessage(DisplayableMessage displayableMessage)
        {
            // only show hard errors
            if (displayableMessage?.Classification == DisplayableMessageClassification.HardError
                || displayableMessage?.Classification == DisplayableMessageClassification.SoftError)
            {
                Logger.Debug("Displaying messages");

                Execute.OnUIThread(
                    () =>
                    {
                        if (_guidInfosToIgnore.Contains(displayableMessage.Id))
                        {
                            Logger.Debug($"Ignoring displayable message ID {displayableMessage.Id}");
                            return;
                        }

                        var message = DisableReasons.FirstOrDefault(s => s.MessageId != null && s.MessageId.Equals(displayableMessage.Id));
                        if (message == null)
                        {
                            DisableReasons.Add(new StatusMessage(displayableMessage, GetAdditionalInfoResourceKey(displayableMessage)));
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

                Execute.OnUIThread(
                    () =>
                    {
                        var reason = DisableReasons.LastOrDefault(d => d.MessageId == displayableMessage.Id);
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
            Execute.OnUIThread(
                () =>
                {
                    Logger.Debug("Clearing messages");

                    DisableReasons.Clear();

                    Logger.Debug("Cleared messages");
                });
        }

        /// <inheritdoc />
        public void DisplayStatus(DisplayableMessage message)
        {
        }

        protected override void OnLoaded()
        {
            MessageDisplay.AddMessageDisplayHandler(this);
            if (DisableByOperatorManager.DisabledByOperator)
            {
                HandleSystemDisabledByOperatorEvent(false);
            }

            IsExitReserveButtonVisible = (bool)PropertiesManager.GetProperty(
                ApplicationConstants.ReserveServiceLockupPresent,
                false);

            EventBus.Subscribe<PropertyChangedEvent>(this, HandleEvent);
            EventBus.Subscribe<SystemEnabledByOperatorEvent>(this, _ => HandleSystemDisabledByOperatorEvent(true));
            EventBus.Subscribe<SystemDisabledByOperatorEvent>(this, _ => HandleSystemDisabledByOperatorEvent(false));
            EventBus.Subscribe<OperatorCultureChangedEvent>(this, HandleOperatorCultureChangedEvent);

            OutOfServiceViewModel.OnLoaded();
        }

        /// <summary>
        ///     Handles unloaded window event.
        /// </summary>
        protected override void OnUnloaded()
        {
            MessageDisplay.RemoveMessageDisplayHandler(this);
            EventBus.Unsubscribe<PropertyChangedEvent>(this);
        }

        protected override void OnInputStatusChanged()
        {
            var active = true;
            var text = string.Empty;
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

            OutOfServiceViewModel.OutOfServiceModeButtonIsEnabled = active;
            InputStatusText = text;

            if (!active && PopupOpen)
            {
                Execute.OnUIThread(
                    () =>
                    {
                        EventBus.Publish(new OperatorMenuPopupEvent(false, string.Empty));
                    });
            }
        }

        private string GetAdditionalInfoResourceKey(DisplayableMessage displayableMessage)
        {
            if (_additionalGuidInfoKeys.TryGetValue(displayableMessage.Id, out var errorKey))
            {
                return errorKey;
            }

            if (displayableMessage.ReasonEvent != null &&
                _additionalTypeInfoKeys.TryGetValue(displayableMessage.ReasonEvent, out var infoKey))
            {
                return infoKey;
            }

            return null;
        }

        private void ExitReserve(object obj)
        {
            EventBus.Publish(new ExitReserveButtonPressedEvent());
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

        private void HandleSystemDisabledByOperatorEvent(bool enabled)
        {
            InputStatusText = enabled ? string.Empty : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutOfService);
        }

        private void HandleOperatorCultureChangedEvent(OperatorCultureChangedEvent evt)
        {
            foreach (var message in DisableReasons)
            {
                message.UpdateAdditionalInfo();
            }
        }
    }
}
