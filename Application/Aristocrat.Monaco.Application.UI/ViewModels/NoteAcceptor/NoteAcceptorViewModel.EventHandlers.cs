namespace Aristocrat.Monaco.Application.UI.ViewModels.NoteAcceptor
{
    using System;
    using System.Globalization;
    using Common;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Helpers;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
#if !RETAIL
    using Vgt.Client12.Testing.Tools;
#endif

    public partial class NoteAcceptorViewModel
    {
        protected override void SubscribeToEvents()
        {
            EventBus.Subscribe<DisabledEvent>(this, HandleHardwareNoteAcceptorDisabledEvent);
            EventBus.Subscribe<EnabledEvent>(this, HandleHardwareNoteAcceptorEnabledEvent);
            EventBus.Subscribe<InspectedEvent>(this, HandleHardwareNoteAcceptorInspectedEvent);
            EventBus.Subscribe<DisconnectedEvent>(this, HandleEvent);
            EventBus.Subscribe<ConnectedEvent>(this, HandleEvent);
            EventBus.Subscribe<HardwareFaultEvent>(this, HandleHardwareFaultEvent);
            EventBus.Subscribe<HardwareFaultClearEvent>(this, HandleHardwareFaultClearEvent);
            EventBus.Subscribe<InspectionFailedEvent>(this, HandleHardwareNoteAcceptorInspectionFailedEvent);
            EventBus.Subscribe<DocumentRejectedEvent>(this, HandleDocumentRejectedEvent);
            EventBus.Subscribe<CurrencyReturnedEvent>(this, HandleCurrencyReturnedEvent);
            EventBus.Subscribe<VoucherReturnedEvent>(this, HandleVoucherReturnedEvent);
            EventBus.Subscribe<CurrencyStackedEvent>(this, HandleCurrencyStackedEvent);
            EventBus.Subscribe<CurrencyEscrowedEvent>(this, HandleCurrencyEscrowedEvent);
            EventBus.Subscribe<VoucherEscrowedEvent>(this, HandleVoucherEscrowedEvent);
            EventBus.Subscribe<SelfTestPassedEvent>(this, HandleHardwareNoteAcceptorSelfTestPassedEvent);
            EventBus.Subscribe<SelfTestFailedEvent>(this, HandleHardwareNoteAcceptorSelfTestFailedEvent);
            EventBus.Subscribe<PropertyChangedEvent>(this, HandleEvent, e => e.PropertyName.Equals(PropertyKey.VoucherIn));
#if !RETAIL
            EventBus.Subscribe<DebugNoteEvent>(this, HandleDebugNoteEvent);
#endif
        }

        /// <summary>
        ///     Subscribes to events and kicks off the UI refresh timer.  The OnRefreshTimeout method, which is called
        ///     each time the timer elapses, will dispatch a call to UpdateScreen, which processes events retrieved from
        ///     the event bus.  UpdateScreen cannot run concurrently with StartEventHandler or StopEventHandler to prevent
        ///     the scenario where UpdateScreen tries to get events and this object is not subscribed to any events.
        /// </summary>
        protected override void StartEventHandler()
        {
            lock (EventLock)
            {
                EventHandlerStopped = false;

                IsVoucherInEnabled = (bool)PropertiesManager.GetProperty(PropertyKey.VoucherIn, false);
                _noteAcceptorDiagnosticsEnabled = GetGlobalConfigSetting(OperatorMenuSetting.HardwareDiagnosticsEnabled, false);

                // Set note acceptor diagnostics as enabled/disabled (used by VoucherIn component to ignore escrow events while in note acceptor screen if enabled).
                PropertiesManager.SetProperty(ApplicationConstants.NoteAcceptorDiagnosticsKey, _noteAcceptorDiagnosticsEnabled);
                Logger.DebugFormat(
                    CultureInfo.CurrentCulture,
                    "Note acceptor diagnostics enabled {0}",
                    _noteAcceptorDiagnosticsEnabled ? "TRUE" : "FALSE");

                UpdateView();
            }

            StartEventHandler(NoteAcceptor);
        }

        /// <summary>
        ///     Stops the UI refresh timer and unsubscribes from all events.  See StartEventHandler.
        /// </summary>
        protected override void StopEventHandler()
        {
            lock (EventLock)
            {
                if (EventHandlerStopped)
                {
                    return;
                }

                if (NoteAcceptor != null)
                {
                    if (NoteAcceptor.IsEscrowed)
                    {
                        NoteAcceptor.Return();
                    }
                }

                if (_noteAcceptorDiagnosticsEnabled)
                {
                    PropertiesManager.SetProperty(ApplicationConstants.NoteAcceptorDiagnosticsKey, false);
                    _noteAcceptorDiagnosticsEnabled =
                        (bool)PropertiesManager.GetProperty(ApplicationConstants.NoteAcceptorDiagnosticsKey, false);
                    Logger.DebugFormat(
                        CultureInfo.CurrentCulture,
                        "Note acceptor diagnostics enabled {0}",
                        _noteAcceptorDiagnosticsEnabled ? "TRUE" : "FALSE");
                }
            }


            string error = string.Empty;
            if (_billAcceptanceLimit > ApplicationConstants.MaxCreditsInMax)
            {
                error = string.Format(Localizer.For(CultureFor.Player).GetString(ResourceKeys.LessThanOrEqualErrorMessage), ApplicationConstants.MaxCreditsInMax.FormattedCurrencyString());
            }

            if (_billAcceptanceLimit < ApplicationConstants.MaxCreditsInMin)
            {
                error = Localizer.For(CultureFor.Player).GetString(ResourceKeys.MaxCreditsInInvalid);
            }
            if (string.IsNullOrEmpty(error))
            {
                if (PropertiesManager.GetValue(PropertyKey.MaxCreditsIn, ApplicationConstants.DefaultMaxCreditsIn).MillicentsToDollars() !=
                    BillAcceptanceLimit)
                {
                    EventBus.Publish(new OperatorMenuSettingsChangedEvent());
                }

                PropertiesManager.SetProperty(PropertyKey.MaxCreditsIn, BillAcceptanceLimit.DollarsToMillicents());
            }

            base.StopEventHandler();
        }

        private void HandleHardwareNoteAcceptorDisabledEvent(DisabledEvent @event)
        {
            if (NoteAcceptor == null)
            {
                Logger.Warn("Note acceptor service unavailable");
                return;
            }

            UpdateWarningMessage();
            SetDiagnosticButtonsEnabled(IsEnableAllowedForTesting());
            UpdateStatus();
        }

        private void HandleHardwareNoteAcceptorEnabledEvent(EnabledEvent enabledEvent)
        {
            if (NoteAcceptor == null)
            {
                Logger.Warn("Note acceptor service unavailable");
                return;
            }

            UpdateWarningMessage();
            UpdateStatus();

            if (_noteAcceptorDiagnosticsEnabled &&
                (enabledEvent.Reasons & EnabledReasons.System) > 0 ||
                (enabledEvent.Reasons & EnabledReasons.Backend) > 0 ||
                (enabledEvent.Reasons & EnabledReasons.Device) > 0 ||
                (enabledEvent.Reasons & EnabledReasons.Configuration) > 0 ||
                (enabledEvent.Reasons & EnabledReasons.Reset) > 0)
            {
                return;
            }

            if (NoteAcceptor.Enabled &&
                NoteAcceptor.LogicalState != NoteAcceptorLogicalState.Uninitialized)
            {
                SetDiagnosticButtonsEnabled(true);
            }
        }

        private void HandleHardwareNoteAcceptorInspectedEvent(InspectedEvent @event)
        {
            _inspecting = false;
            RemoveStatusMessage(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionFailedText));
            LastDocumentResultText = NoteAcceptor.LastDocumentResult.ToString();
            StackerStateText = NoteAcceptor.StackerState.ToString();
            ConfigureDenominations();
            SetDeviceInformation();

            SetDenominationsVisible(IsDenominationsVisible);
        }

        private void HandleEvent(PropertyChangedEvent @event)
        {
            UpdateScreen();
        }

        private void HandleEvent(DisconnectedEvent @event)
        {
            SetDiagnosticButtonsEnabled(false);
            AddStatusMessage(@event.ToString(), StatusMode.Error);
            SetDenominationsVisible(IsDenominationsVisible);
            ConfigureDenominations();
        }

        private void HandleEvent(ConnectedEvent @event)
        {
            SetDiagnosticButtonsEnabled(true);
            RemoveStatusMessage(DisconnectedEvent.NoteAcceptorDisconnectedText);
            SetDenominationsVisible(IsDenominationsVisible);
            ConfigureDenominations();
        }

        private void HandleCurrencyEscrowedEvent(CurrencyEscrowedEvent escrowedCurrencyEvent)
        {
            UpdateStatus(
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EscrowedAmountText) +
                escrowedCurrencyEvent.Note.Value.FormattedCurrencyString());
        }

        private void HandleCurrencyReturnedEvent(CurrencyReturnedEvent @event)
        {
            UpdateStatus(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReturnedText));
        }

        private void HandleCurrencyStackedEvent(CurrencyStackedEvent @event)
        {
            UpdateStatus(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StackedText));
        }

#if !RETAIL
        private void HandleDebugNoteEvent(DebugNoteEvent @event)
        {
            ReturnButtonVisible = true;
        }
#endif

        private void HandleDocumentRejectedEvent(DocumentRejectedEvent @event)
        {
            UpdateStatus(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RejectedText));
        }

        private void HandleVoucherEscrowedEvent(VoucherEscrowedEvent escrowedVoucherEvent)
        {
            UpdateStatus(
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EscrowedVoucherText) +
                escrowedVoucherEvent.Barcode.GetFormattedBarcode());
        }

        private void HandleVoucherReturnedEvent(VoucherReturnedEvent @event)
        {
            UpdateStatus(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReturnedText));
        }

        private void HandleHardwareFaultEvent(HardwareFaultEvent @event)
        {
            foreach (NoteAcceptorFaultTypes value in Enum.GetValues(typeof(NoteAcceptorFaultTypes)))
            {
                if (!@event.Fault.HasFlag(value))
                {
                    continue;
                }

                if (_faultTexts.ContainsKey(value))
                {
                    UpdateStatus();
                }
                else
                {
                    Logger.Warn($"Unhandled HardwareFaultEvent {value}");
                }
            }
        }

        private void HandleHardwareFaultClearEvent(HardwareFaultClearEvent @event)
        {
            foreach (NoteAcceptorFaultTypes value in Enum.GetValues(typeof(NoteAcceptorFaultTypes)))
            {
                if (!@event.Fault.HasFlag(value))
                {
                    continue;
                }

                if (_faultTexts.ContainsKey(value))
                {
                    UpdateStatus();
                }
                else
                {
                    Logger.Warn($"Unhandled HardwareFaultClearEvent {value}");
                }
            }

            if (@event.Fault.HasFlag(NoteAcceptorFaultTypes.StackerDisconnected) ||
               @event.Fault.HasFlag(NoteAcceptorFaultTypes.NoteJammed) ||
               (@event.Fault & NoteAcceptorFaultTypes.OtherFault) != 0)
            {
                SetDiagnosticButtonsEnabled(true);
            }
        }

        private void HandleHardwareNoteAcceptorInspectionFailedEvent(InspectionFailedEvent @event)
        {
            _inspecting = false;
            AddStatusMessage(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionFailedText), StatusMode.Error);
            Inspection?.ReportTestFailure();
            SetDeviceInformation();
        }

        private void HandleHardwareNoteAcceptorSelfTestPassedEvent(SelfTestPassedEvent @event)
        {
            SelfTestCurrentState = SelfTestState.Passed;
            SetDeviceInformation();
            UpdateStatus();
            SetDiagnosticButtonsEnabled(true);
        }

        private void HandleHardwareNoteAcceptorSelfTestFailedEvent(SelfTestFailedEvent @event)
        {
            SelfTestCurrentState = SelfTestState.Failed;
            Inspection?.ReportTestFailure();
            SetDiagnosticButtonsEnabled(true);
        }
    }
}
