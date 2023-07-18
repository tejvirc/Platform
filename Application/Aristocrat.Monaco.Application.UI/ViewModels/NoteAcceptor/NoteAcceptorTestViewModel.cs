namespace Aristocrat.Monaco.Application.UI.ViewModels.NoteAcceptor
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.ConfigWizard;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Monaco.Localization.Properties;
    using OperatorMenu;
#if !RETAIL
    using Vgt.Client12.Testing.Tools;
#endif

    [CLSCompliant(false)]
    public class NoteAcceptorTestViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly INoteAcceptor _noteAcceptor;
        private string _status;

        private bool _disabledByBackend;
        private bool _disabledByConfiguration;
        private bool _disabledByDevice;
        private bool _disabledBySystem;
        private bool _disabledByGame;
        private IInspectionService _reporter;

        public NoteAcceptorTestViewModel()
            : this(ServiceManager.GetInstance().TryGetService<IInspectionService>(),
                  ServiceManager.GetInstance().TryGetService<INoteAcceptor>())
        {
        }

        public NoteAcceptorTestViewModel(IInspectionService reporter, INoteAcceptor noteAcceptor)
        {
            _reporter = reporter; // it's non-null only for Inspection Tool
            _noteAcceptor = noteAcceptor ?? throw new ArgumentNullException(nameof(noteAcceptor));
            _status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReadyToInsert);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value, nameof(Status));
        }

        public ObservableCollection<string> TestEvents { get; } = new ObservableCollection<string>();

        protected override bool CloseOnRestrictedAccess => _reporter is null;

        protected override void OnLoaded()
        {
            base.OnLoaded();
            SubscribeToEvents();
            SetEnableReason();
        }

        protected override void OnUnloaded()
        {
            ResetDisabledReasons();
            base.OnUnloaded();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<ConnectedEvent>(this, HandleStatusEvent);
            EventBus.Subscribe<DisabledEvent>(this, HandleStatusEvent);
            EventBus.Subscribe<DisconnectedEvent>(this, HandleStatusEvent);
            EventBus.Subscribe<EnabledEvent>(this, HandleStatusEvent);

            EventBus.Subscribe<CurrencyEscrowedEvent>(this, HandleEvent);
            EventBus.Subscribe<DocumentRejectedEvent>(this, HandleEvent);
            EventBus.Subscribe<VoucherEscrowedEvent>(this, HandleEvent);

#if !RETAIL
            EventBus.Subscribe<DebugNoteEvent>(this, HandleEvent);
#endif
        }

        private void HandleStatusEvent(NoteAcceptorBaseEvent evt)
        {
            if (_noteAcceptor == null)
            {
                Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText);
                return;
            }

            if (evt is EnabledEvent enabledEvent && enabledEvent.Reasons == EnabledReasons.Reset)
            {
                ResetDisabledReasons();
                SetEnableReason();
            }

            switch (_noteAcceptor.LogicalState)
            {
                case NoteAcceptorLogicalState.Idle:
                    Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReadyToInsert);
                    break;
                case NoteAcceptorLogicalState.Disabled:
                    Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disabled);
                    break;
                case NoteAcceptorLogicalState.Disconnected:
                    Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disconnected);
                    break;
                case NoteAcceptorLogicalState.Uninitialized:
                    Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                    break;
                default:
                    Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Pending);
                    break;
            }
        }

        private void HandleEvent(CurrencyEscrowedEvent evt)
        {
            var eventName =
                $"{evt.Note.Value.FormattedCurrencyString("C0")} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BillInserted)}";

            Execute.OnUIThread(
                () => TestEvents.Insert(
                    0,
                    eventName));

            _reporter?.SetTestName(eventName);
            Task.Run(ReturnWithDelay);
        }

        private void ReturnWithDelay()
        {
            Thread.Sleep(2000);

            _noteAcceptor.Return();
        }

        private void HandleEvent(DocumentRejectedEvent evt)
        {
            var eventName = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidDocInserted);

            Execute.OnUIThread(
                () => TestEvents.Insert(
                    0,
                    eventName));

            _reporter?.SetTestName(eventName);
        }

        private void HandleEvent(VoucherEscrowedEvent evt)
        {
            var eventName =
                $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherInserted)}\r{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ValidationNumber)} {evt.Barcode}";
            Execute.OnUIThread(
                () => TestEvents.Insert(
                    0,
                    eventName));

            _reporter?.SetTestName(eventName);
            Task.Run(ReturnWithDelay);
        }

#if !RETAIL
        private void HandleEvent(DebugNoteEvent evt)
        {
            Execute.OnUIThread(
                () => TestEvents.Insert(
                    0,
                    $"{evt.Denomination.FormattedCurrencyString("C0")} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BillInserted)}"));

            Task.Run(ReturnWithDelay);
        }
#endif

        private void SetEnableReason()
        {
            if (_noteAcceptor.LogicalState != NoteAcceptorLogicalState.Uninitialized)
            {
                if (!_noteAcceptor.Enabled)
                {
                    if ((_noteAcceptor.ReasonDisabled & DisabledReasons.System) > 0
                        || (_noteAcceptor.ReasonDisabled & DisabledReasons.Backend) > 0
                        || (_noteAcceptor.ReasonDisabled & DisabledReasons.Device) > 0
                        || (_noteAcceptor.ReasonDisabled & DisabledReasons.Configuration) > 0
                        || (_noteAcceptor.ReasonDisabled & DisabledReasons.GamePlay) > 0)
                    {
                        Logger.Debug("Enable note acceptor");

                        if ((_noteAcceptor.ReasonDisabled & DisabledReasons.System) > 0)
                        {
                            _noteAcceptor.Enable(EnabledReasons.System);
                            _disabledBySystem = true;
                        }

                        if ((_noteAcceptor.ReasonDisabled & DisabledReasons.Backend) > 0)
                        {
                            _noteAcceptor.Enable(EnabledReasons.Backend);
                            _disabledByBackend = true;
                        }

                        if ((_noteAcceptor.ReasonDisabled & DisabledReasons.Device) > 0)
                        {
                            _noteAcceptor.Enable(EnabledReasons.Device);
                            _disabledByDevice = true;
                        }

                        if ((_noteAcceptor.ReasonDisabled & DisabledReasons.Configuration) > 0)
                        {
                            _noteAcceptor.Enable(EnabledReasons.Configuration);
                            _disabledByConfiguration = true;
                        }

                        if ((_noteAcceptor.ReasonDisabled & DisabledReasons.GamePlay) > 0 && GameIdle)
                        {
                            _noteAcceptor.Enable(EnabledReasons.GamePlay);
                            _disabledByGame = true;
                        }
                    }
                }
            }
        }

        private void ResetDisabledReasons()
        {
            if (_disabledBySystem)
            {
                Logger.Debug("Note acceptor disabled by system");
                _noteAcceptor.Disable(DisabledReasons.System);
                _disabledBySystem = false;
            }

            if (_disabledByBackend)
            {
                Logger.Debug("Note acceptor disabled by backend");
                _noteAcceptor.Disable(DisabledReasons.Backend);
                _disabledByBackend = false;
            }

            if (_disabledByDevice && !_noteAcceptor.Connected)
            {
                Logger.DebugFormat(CultureInfo.CurrentCulture, "Note acceptor disabled by device");
                _noteAcceptor.Disable(DisabledReasons.Device);
                _disabledByDevice = false;
            }

            if (_disabledByConfiguration)
            {
                Logger.Debug("Note acceptor disabled by configuration");
                _noteAcceptor.Disable(DisabledReasons.Configuration);
                _disabledByConfiguration = false;
            }

            if (_disabledByGame)
            {
                Logger.Debug("Note acceptor disabled by gamePlay");
                _noteAcceptor.Disable(DisabledReasons.GamePlay);
                _disabledByGame = false;
            }
        }
    }
}
