namespace Aristocrat.Monaco.Hardware.NoteAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.Dfu;
    using Contracts.NoteAcceptor;
    using Contracts.Persistence;
    using Contracts.SerialPorts;
    using Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using Stateless;
    using Constants = Kernel.Contracts.Components.Constants;

    /// <summary>A note acceptor adapter.</summary>
    /// <seealso cref="INoteAcceptorImplementation" />
    /// <seealso cref="INoteAcceptor" />
    /// <seealso cref="NoteAcceptorOptions" />
    public class NoteAcceptorAdapter : DeviceAdapter<INoteAcceptorImplementation>,
        INoteAcceptor,
        IStorageAccessor<NoteAcceptorOptions>
    {
        private const string OptionsBlock = "Aristocrat.Monaco.Hardware.NoteAcceptor.NoteAcceptorAdapter.Options";
        private const string LastConfigurationBlock = "Aristocrat.Monaco.Hardware.LastDeviceConfiguration";
        private const string SupportedNotesKey = "SupportedNotes";
        private const string LastDocumentResultOption = "LastDocumentResult";
        private const string WasStackingOnLastPowerUpOption = "WasStackingOnLastPowerUp";
        private const string ActivationTimeText = "ActivationTime";
        private const string NoteAcceptor = "Note Acceptor";
        private const int SelfTestInterval = 5000;
        private const int MaxSelfTestRetries = 5;
        private const int SelfTestTimeout = 20;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IPersistentStorageAccessor _accessor;
        private readonly StateMachine<NoteAcceptorLogicalState, NoteAcceptorLogicalStateTrigger> _state;
        private readonly object _instanceLock = new();
        private readonly object _noteLock = new();
        private readonly object _denomLock = new();
        private readonly object _activeDenominationsLock = new();
        private readonly IPersistentStorageManager _storageManager;
        private readonly IDisabledNotesService _disabledNotesService;
        private readonly IPersistenceProvider _persistence;
        private readonly Collection<int> _disabledNotes = new();
        private readonly List<(int Denom, string IsoCode)> _excludedNotes = new();

        private ReaderWriterLockSlim _stateLock = new(LockRecursionPolicy.SupportsRecursion);
        private AutoResetEvent _selfTestWaitHandle = new(true);

        private IPersistentBlock _supportedNotesPersistentBlock;
        private bool _validatingDevice;
        private int _selfTestRetryCount;
        private HashSet<int> _denominations = new();
        private DateTime _activationTime = DateTime.MinValue;
        private DocumentResult _lastResult = DocumentResult.None;
        private bool _wasStackingOnLastPowerUp;
        private string _configuration;
        private INoteAcceptorImplementation _noteAcceptor;
        private string _isoCode;
        private Collection<int> _supportedNotes = new();

        private EventWaitHandle _stackingEventWaitHandle;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.NoteAcceptor.NoteAcceptorAdapter class.
        /// </summary>
        public NoteAcceptorAdapter(
            IEventBus eventBus,
            IComponentRegistry componentRegistry,
            IDfuProvider dfuProvider,
            IPersistentStorageManager storageManager,
            IDisabledNotesService disabledNotesService,
            IPersistenceProvider persistence,
            ISerialPortsService serialPortsService,
            INoteAcceptorImplementation implementation)
            : base(eventBus, componentRegistry, dfuProvider, serialPortsService)
        {
            _state = ConfigureStateMachine();
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _accessor = _storageManager.GetAccessor(PersistenceLevel.Transient, LastConfigurationBlock);
            _configuration = (string)_accessor["Configuration"];
            _disabledNotesService =
                disabledNotesService ?? throw new ArgumentNullException(nameof(disabledNotesService));
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _noteAcceptor = implementation ?? throw new ArgumentNullException(nameof(implementation));
            Disable(DisabledReasons.Device);
        }

        /// <summary>
        ///     Gets or sets the current note acceptor configuration - model, vendor, protocol, firmware
        ///     combined into a string
        /// </summary>
        public string Configuration
        {
            get => _configuration;
            set
            {
                var changed = !value.Equals(_configuration);
                Logger.Debug($"Note Acceptor Configuration has changed is {changed} for configuration '{value}'");
                if (changed)
                {
                    _configuration = value;
                    PostEvent(new NoteAcceptorChangedEvent(NoteAcceptorId));
                    PersistConfiguration();
                }
            }
        }

#if !(RETAIL)
        /// <summary>
        ///     Get NoteAcceptor implementation for automation.
        /// </summary>
        public INoteAcceptorImplementation NoteAcceptorImplementation => Implementation;
#endif

        /// <inheritdoc />
        protected override INoteAcceptorImplementation Implementation => _noteAcceptor;

        /// <inheritdoc />
        protected override string Description => NoteAcceptor;

        /// <inheritdoc />
        protected override string Path => Constants.NoteAcceptorPath;

        public override DeviceType DeviceType => DeviceType.NoteAcceptor;

        /// <inheritdoc />
        public int NoteAcceptorId { get; set; } = 1; // Default to deviceId 1 since 0 isn't valid in G2S

        /// <inheritdoc />
        public bool CanValidate => CanFire(NoteAcceptorLogicalStateTrigger.Escrowed);

        /// <inheritdoc />
        public bool IsReady { get; private set; }

        /// <inheritdoc />
        public bool IsEscrowed => CanFire(NoteAcceptorLogicalStateTrigger.Stacking);

        /// <inheritdoc />
        public bool WasStackingOnLastPowerUp
        {
            get => _wasStackingOnLastPowerUp;
            private set
            {
                if (_wasStackingOnLastPowerUp != value)
                {
                    _wasStackingOnLastPowerUp = value;
                    this.ModifyBlock(
                        _storageManager,
                        OptionsBlock,
                        (transaction, index) =>
                        {
                            transaction[index, WasStackingOnLastPowerUpOption] = _wasStackingOnLastPowerUp;
                            return true;
                        },
                        NoteAcceptorId - 1);
                }
            }
        }

        /// <inheritdoc />
        public List<int> Denominations
        {
            get
            {
                lock (_denomLock)
                {
                    return _denominations.ToList();
                }
            }
        }

        /// <inheritdoc />
        public DateTime ActivationTime
        {
            get => _activationTime;
            set
            {
                if (_activationTime != value)
                {
                    _activationTime = value;
                    this.ModifyBlock(
                        _storageManager,
                        OptionsBlock,
                        (transaction, index) =>
                        {
                            transaction[index, ActivationTimeText] = _activationTime;
                            return true;
                        },
                        NoteAcceptorId - 1);
                }
            }
        }

        /// <inheritdoc />
        public DocumentResult LastDocumentResult
        {
            get => _lastResult;
            protected set
            {
                if (_lastResult != value)
                {
                    _lastResult = value;
                    this.ModifyBlock(
                        _storageManager,
                        OptionsBlock,
                        (transaction, index) =>
                        {
                            transaction[index, LastDocumentResultOption] = (int)_lastResult;
                            return true;
                        },
                        NoteAcceptorId - 1);
                }
            }
        }

        /// <inheritdoc />
        public NoteAcceptorStackerState StackerState
        {
            get
            {
                // TODO: This isn't ideal, but it prevents us from having to maintain multiple states
                if (Faults.HasFlag(NoteAcceptorFaultTypes.StackerDisconnected))
                {
                    return NoteAcceptorStackerState.Removed;
                }

                if (Faults.HasFlag(NoteAcceptorFaultTypes.StackerFault))
                {
                    return NoteAcceptorStackerState.Fault;
                }

                if (Faults.HasFlag(NoteAcceptorFaultTypes.StackerJammed))
                {
                    return NoteAcceptorStackerState.Jammed;
                }

                if (Faults.HasFlag(NoteAcceptorFaultTypes.StackerFull))
                {
                    return NoteAcceptorStackerState.Full;
                }

                return NoteAcceptorStackerState.Inserted;
            }
        }

        /// <inheritdoc />
        public override bool Connected => !(_state?.State == NoteAcceptorLogicalState.Uninitialized ||
                                            _state?.State == NoteAcceptorLogicalState.Disconnected ||
                                            (!Implementation?.IsConnected ?? false));

        /// <inheritdoc />
        public NoteAcceptorLogicalState LogicalState => _state.State;

        /// <inheritdoc />
        public NoteAcceptorFaultTypes Faults => Implementation?.Faults ?? NoteAcceptorFaultTypes.None;

        /// <inheritdoc />
        public override string Name =>
            string.IsNullOrEmpty(ServiceProtocol) == false
                ? $"{ServiceProtocol} Note Acceptor Service"
                : "Unknown Note Acceptor Service";

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes => new[] { typeof(INoteAcceptor) };

        public bool DenomIsValid(int value)
        {
            return Denominations.Contains(value);
        }

        /// <inheritdoc />
        public Collection<int> GetSupportedNotes(string isoCode = null)
        {
            lock (_noteLock)
            {
                if (_supportedNotes.Count == 0 || isoCode != null)
                {
                    var supportedNotes = new Collection<int>();

                    if (isoCode == null)
                    {
                        isoCode = _isoCode;
                    }

                    Logger.Info($"Get Supported notes for ISO {isoCode}");

                    if (_supportedNotesPersistentBlock.GetValue(SupportedNotesKey, out NoteInfo noteInfo))
                    {
                        foreach (var denom in noteInfo.Notes.Select(a => a)
                                     .Where(a => string.IsNullOrEmpty(isoCode) || a.IsoCode == isoCode)
                                     .Select(a => a.Denom))
                        {
                            if (!_excludedNotes.Any(a => a.Denom == denom && a.IsoCode == _isoCode))
                            {
                                if (!supportedNotes.Contains(denom))
                                {
                                    Logger.Info($"Added supported note {denom} for ISO {isoCode}");
                                    supportedNotes.Add(denom);
                                }
                            }
                        }
                    }

                    _supportedNotes = supportedNotes;
                }
            }

            return _supportedNotes;
        }

        /// <inheritdoc />
        public List<ISOCurrencyCode> GetSupportedCurrencies()
        {
            var currencies = Implementation?.SupportedNotes?.Select(n => n.CurrencyCode).Distinct();

            return currencies?.ToList();
        }

        /// <inheritdoc />
        public void SetIsoCode(string isoCode)
        {
            lock (_noteLock)
            {
                _isoCode = isoCode;
            }

            SetActiveDenominations(isoCode);
        }

        /// <inheritdoc />
        public void UpdateDenom(int denom, bool enabled = true)
        {
            if (!_supportedNotes.Contains(denom))
            {
                return;
            }

            lock (_noteLock)
            {
                var noteInfo = _disabledNotesService.NoteInfo;
                if (noteInfo != null)
                {
                    if (enabled)
                    {
                        noteInfo.Notes = noteInfo.Notes.Select(a => a).Where(a => a.Denom != denom).ToArray();
                    }
                    else
                    {
                        var notes = noteInfo.Notes.ToList();
                        notes.Add((denom, _isoCode));
                        noteInfo.Notes = notes.ToArray();
                    }

                    _disabledNotesService.NoteInfo = noteInfo;

                    _disabledNotes.Clear();
                    foreach (var note in noteInfo.Notes)
                    {
                        _disabledNotes.Add(note.Denom);
                    }
                }
            }

            SetActiveDenominations();
        }

        /// <inheritdoc />
        public void SetNoteDefinitions(Collection<NoteDefinitions> notesDefinitions)
        {
            if (notesDefinitions == null)
            {
                return;
            }

            lock (_noteLock)
            {
                _excludedNotes.Clear();
                foreach (var currency in notesDefinitions)
                {
                    foreach (var denom in currency.ExcludedDenominations)
                    {
                        _excludedNotes.Add((denom, currency.Code));

                        if (_isoCode != null && _isoCode.Equals(currency.Code) && _supportedNotes.Count > 0)
                        {
                            _supportedNotes.Remove(denom);
                        }
                    }
                }
            }

            if (_supportedNotes != null && _supportedNotes.Count > 0)
            {
                SetActiveDenominations();
            }
        }

        /// <inheritdoc />
        public bool IsNoteDisabled(int denom)
        {
            var noteDisabled = false;
            var noteInfo = _disabledNotesService.NoteInfo;
            if (noteInfo != null)
            {
                noteDisabled = noteInfo.Notes.Any(a => a.Denom == denom);
            }

            return noteDisabled;
        }

        /// <inheritdoc />
        public override async Task<bool> SelfTest(bool clear)
        {
            WasStackingOnLastPowerUp = false;
            return await SelfTestInternal(clear);
        }

        /// <inheritdoc />
        public virtual async Task<bool> AcceptNote()
        {
            if (!IsEscrowed)
            {
                return false;
            }

            return await StackDocument(Implementation.AcceptNote);
        }

        /// <inheritdoc />
        public virtual async Task<bool> AcceptTicket()
        {
            if (!IsEscrowed)
            {
                return false;
            }

            return await StackDocument(Implementation.AcceptTicket);
        }

        public void Inspect(int timeout)
        {
            Inspect(LastComConfiguration, timeout);
        }

        /// <inheritdoc />
        public virtual async Task<bool> Return()
        {
            Fire(NoteAcceptorLogicalStateTrigger.Returning);
            return await Implementation.Return();
        }

        /// <inheritdoc />
        public bool TryAddBlock(IPersistentStorageAccessor accessor, int blockIndex, out NoteAcceptorOptions block)
        {
            block = new NoteAcceptorOptions();

            using (var transaction = accessor.StartTransaction())
            {
                transaction[blockIndex, LastDocumentResultOption] = (int)block.LastDocumentResult;

                transaction.Commit();
                return true;
            }
        }

        /// <inheritdoc />
        public bool TryGetBlock(IPersistentStorageAccessor accessor, int blockIndex, out NoteAcceptorOptions block)
        {
            block = new NoteAcceptorOptions
            {
                LastDocumentResult = (DocumentResult)(int)accessor[blockIndex, LastDocumentResultOption],
                ActivationTime = (DateTime)accessor[blockIndex, ActivationTimeText],
                WasStackingOnLastPowerUp = (bool)accessor[blockIndex, WasStackingOnLastPowerUpOption]
            };

            return true;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            lock (_instanceLock)
            {
                if (Disposed)
                {
                    return;
                }

                if (disposing)
                {
                    Disable(DisabledReasons.Service);
                    if (Implementation != null)
                    {
                        Implementation.Connected -= ImplementationConnected;
                        Implementation.Disconnected -= ImplementationDisconnected;
                        Implementation.Initialized -= ImplementationInitialized;
                        Implementation.InitializationFailed -= ImplementationInitializationFailed;
                        Implementation.Disabled -= ImplementationDisabled;
                        Implementation.Enabled -= ImplementationEnabled;
                        Implementation.FaultCleared -= ImplementationFaultCleared;
                        Implementation.FaultOccurred -= ImplementationFaultOccurred;
                        Implementation.NoteOrTicketRejected -= ImplementationNoteOrTicketRejected;
                        Implementation.NoteOrTicketRemoved -= ImplementationNoteOrTicketRemoved;
                        Implementation.UnknownDocumentReturned -= ImplementationNoteOrTicketReturned;
                        Implementation.NoteAccepted -= ImplementationNoteAccepted;
                        Implementation.NoteReturned -= ImplementationNoteReturned;
                        Implementation.NoteValidated -= ImplementationNoteValidated;
                        Implementation.NoteOrTicketStacking -= ImplementationTicketStacking;
                        Implementation.TicketAccepted -= ImplementationTicketAccepted;
                        Implementation.TicketReturned -= ImplementationTicketReturned;
                        Implementation.TicketValidated -= ImplementationTicketValidated;
                        Implementation.Dispose();
                        _noteAcceptor = null;
                    }

                    if (_stackingEventWaitHandle != null)
                    {
                        _stackingEventWaitHandle.Set();
                        _stackingEventWaitHandle.Dispose();
                    }

                    _stateLock.Dispose();
                    _stateLock = null;

                    if (_selfTestWaitHandle != null)
                    {
                        _selfTestWaitHandle.Set();
                        _selfTestWaitHandle.Dispose();
                        _selfTestWaitHandle = null;
                    }
                }

                _stackingEventWaitHandle = null;

                base.Dispose(disposing);
            }
        }

        /// <inheritdoc />
        protected override void DisabledDetected()
        {
            PostEvent(new DisabledEvent(NoteAcceptorId, ReasonDisabled));
        }

        /// <inheritdoc />
        protected override void Disabling(DisabledReasons reason)
        {
            if (Fire(NoteAcceptorLogicalStateTrigger.Disable, new DisabledEvent(NoteAcceptorId, ReasonDisabled), true))
            {
                Implementation?.Disable();
            }
        }

        /// <inheritdoc />
        protected override void Enabling(EnabledReasons reason, DisabledReasons remedied)
        {
            CheckActivationTime();

            if (Enabled)
            {
                if (Fire(NoteAcceptorLogicalStateTrigger.Enable, new EnabledEvent(NoteAcceptorId, reason), true))
                {
                    Implementation?.Enable();
                }
                else
                {
                    PostEvent(new EnabledEvent(NoteAcceptorId, reason));
                }
            }
            else
            {
                if (reason == EnabledReasons.Device &&
                    ((remedied & DisabledReasons.GamePlay) != 0 ||
                     (ReasonDisabled & DisabledReasons.GamePlay) != 0 ||
                     (ReasonDisabled & DisabledReasons.System) != 0))
                {
                    PostEvent(new EnabledEvent(reason));
                }

                DisabledDetected();
            }
        }

        /// <inheritdoc />
        protected override void Initializing()
        {
            lock (_instanceLock)
            {
                if (Implementation == null)
                {
                    var errorMessage = $"Cannot load {Name}";
                    Logger.Fatal(errorMessage);
                    throw new ServiceException(errorMessage);
                }

                _supportedNotesPersistentBlock = _persistence.GetOrCreateBlock(
                    SupportedNotesKey,
                    PersistenceLevel.Critical);

                ReadOrCreateOptions();
                HandlePowerUpStacking();

                Implementation.Connected += ImplementationConnected;
                Implementation.Disconnected += ImplementationDisconnected;
                Implementation.Initialized += ImplementationInitialized;
                Implementation.InitializationFailed += ImplementationInitializationFailed;
                Implementation.Disabled += ImplementationDisabled;
                Implementation.Enabled += ImplementationEnabled;
                Implementation.FaultCleared += ImplementationFaultCleared;
                Implementation.FaultOccurred += ImplementationFaultOccurred;
                Implementation.NoteOrTicketRejected += ImplementationNoteOrTicketRejected;
                Implementation.NoteOrTicketRemoved += ImplementationNoteOrTicketRemoved;
                Implementation.UnknownDocumentReturned += ImplementationNoteOrTicketReturned;
                Implementation.NoteAccepted += ImplementationNoteAccepted;
                Implementation.NoteReturned += ImplementationNoteReturned;
                Implementation.NoteValidated += ImplementationNoteValidated;
                Implementation.NoteOrTicketStacking += ImplementationTicketStacking;
                Implementation.TicketAccepted += ImplementationTicketAccepted;
                Implementation.TicketReturned += ImplementationTicketReturned;
                Implementation.TicketValidated += ImplementationTicketValidated;
            }
        }

        /// <inheritdoc />
        protected override void Inspecting(IComConfiguration comConfiguration, int timeout)
        {
            Fire(NoteAcceptorLogicalStateTrigger.Inspecting);
        }

        /// <inheritdoc />
        protected override void SubscribeToEvents(IEventBus eventBus)
        {
        }

        private void CheckActivationTime()
        {
            if (ActivationTime == DateTime.MinValue)
            {
                ActivationTime = DateTime.UtcNow;
            }
        }

        private async Task<bool> SelfTestInternal(bool clear, bool retryOnFailure = false)
        {
            if (!_selfTestWaitHandle.WaitOne(SelfTestTimeout))
            {
                // there is another self test running
                Logger.Debug("Another self test is running");
                return false;
            }

            var result = await base.SelfTest(clear);
            if (result)
            {
                try
                {
                    Logger.Debug("Self test passed");
                    PostEvent(new SelfTestPassedEvent(NoteAcceptorId));
                    ClearSelfTestFailed();
                }
                finally
                {
                    _selfTestWaitHandle.Set();
                }
            }
            else
            {
                try
                {
                    SelfTestFailed();
                }
                finally
                {
                    _selfTestWaitHandle.Set();
                }
                
                if (retryOnFailure)
                {
                    if (++_selfTestRetryCount < MaxSelfTestRetries && (Implementation?.IsConnected ?? false))
                    {
                        RetrySelfTest();

                        void RetrySelfTest()
                        {
                            Task.Delay(SelfTestInterval).ContinueWith(_ => SelfTestInternal(clear, true));
                        }
                    }
                }
            }

            return result;
        }

        private void ClearSelfTestFailed()
        {
            var error = typeof(SelfTestFailedEvent);
            if (ClearError(error))
            {
                Logger.Info($"RequirementsCleared: REMOVED {error} from the error list.");
                if (!AnyErrors)
                {
                    Enable(EnabledReasons.Reset);
                }
            }

            if (ReasonDisabled.HasFlag(DisabledReasons.Device))
            {
                Enable(EnabledReasons.Device);
            }

            IsReady = true;
        }

        private void SelfTestFailed()
        {
            Logger.Error("Self test failed");

            PostEvent(new SelfTestFailedEvent(NoteAcceptorId));

            var error = typeof(SelfTestFailedEvent);
            if (Implementation == null || !AddError(error))
            {
                return;
            }

            LastDocumentResult = DocumentResult.None;
            IsReady = false;
            Logger.Warn($"ResolverError: ADDED {error} to the error list.");
            Disable(DisabledReasons.Error);
        }

        private void ReadOrCreateOptions()
        {
            if (!this.GetOrAddBlock(
                    _storageManager,
                    OptionsBlock,
                    out var options,
                    NoteAcceptorId - 1,
                    PersistenceLevel.Critical))
            {
                Logger.Error($"Could not access block {OptionsBlock} {NoteAcceptorId - 1}");
                return;
            }

            _lastResult = options.LastDocumentResult;
            _activationTime = options.ActivationTime;
            _wasStackingOnLastPowerUp = options.WasStackingOnLastPowerUp;
            Logger.Debug($"Block successfully read {OptionsBlock} {NoteAcceptorId - 1}");
        }

        private void HandlePowerUpStacking()
        {
            if (LastDocumentResult == DocumentResult.Stacking)
            {
                WasStackingOnLastPowerUp = true;
                LastDocumentResult = DocumentResult.None;
            }
        }

        private void ImplementationConnected(object sender, EventArgs e)
        {
            Logger.Info("ImplementationConnected: device connected");
            Fire(NoteAcceptorLogicalStateTrigger.Connected, new ConnectedEvent(NoteAcceptorId));

            Task.Delay(SelfTestInterval).ContinueWith(_ => CheckDevice());
        }

        private void CheckDevice()
        {
            if (_validatingDevice)
            {
                return;
            }

            try
            {
                _validatingDevice = true;
                _selfTestRetryCount = 0;
                if (Implementation != null &&
                    Implementation.IsConnected &&
                    CalculateCrc(0).Result != 0 &&
                    SelfTestInternal(false, true).Result)
                {
                    if (ReasonDisabled.HasFlag(DisabledReasons.Device))
                    {
                        Enable(EnabledReasons.Device);
                    }
                }
            }
            finally
            {
                _validatingDevice = false;
            }
        }

        private void ImplementationDisconnected(object sender, EventArgs e)
        {
            Logger.Warn("ImplementationDisconnected: device disconnected");
            _stackingEventWaitHandle?.Reset();
            Disable(DisabledReasons.Device);
            Fire(NoteAcceptorLogicalStateTrigger.Disconnected, new DisconnectedEvent(NoteAcceptorId));
            UnregisterComponent();
        }

        private void ImplementationInitialized(object sender, EventArgs e)
        {
            if (!CanFire(NoteAcceptorLogicalStateTrigger.Initialized))
            {
                Logger.Error("ImplementationInitialized: invalid state for device initialized");

                return;
            }

            Logger.Info("ImplementationInitialized: device initialized");

            SaveSupportedNotes();

            Implementation?.UpdateConfiguration(InternalConfiguration);

            Configuration = GetConfigurationString();

            RegisterComponent();

            Fire(NoteAcceptorLogicalStateTrigger.Initialized, new InspectedEvent(NoteAcceptorId));
            Initialized = true;

            if ((ReasonDisabled & DisabledReasons.Error) != 0)
            {
                if (!AnyErrors)
                {
                    Enable(EnabledReasons.Reset);
                }
            }

            if (Enabled)
            {
                Implementation?.Enable();
            }
            else
            {
                DisabledDetected();
                Implementation?.Disable();
            }
        }

        private Collection<int> GetDisabledNotes(string isoCode = null)
        {
            lock (_noteLock)
            {
                isoCode ??= _isoCode;
                if (_disabledNotes.Count != 0)
                {
                    return _disabledNotes;
                }

                var noteInfo = _disabledNotesService.NoteInfo;
                if (noteInfo != null)
                {
                    foreach (var denom in noteInfo.Notes.Select(a => a).Where(a => a.IsoCode == isoCode)
                                 .Select(a => a.Denom))
                    {
                        _disabledNotes.Add(denom);
                    }
                }
                else
                {
                    noteInfo = new NoteInfo { Notes = Array.Empty<(int, string)>() };
                    _disabledNotesService.NoteInfo = noteInfo;
                }
            }

            return _disabledNotes;
        }

        private void SaveSupportedNotes()
        {
            if (Implementation == null ||
                !Implementation.SupportedNotes.Any() && !Implementation.ReadNoteTable().Result)
            {
                return;
            }

            var supportedNotes = new NoteInfo
            {
                Notes = Implementation.SupportedNotes.Select(a => (a.Value, a.CurrencyCode.ToString())).ToArray()
            };
            var notesUpdated = false;
            if (_supportedNotesPersistentBlock.GetValue(SupportedNotesKey, out NoteInfo noteInfo))
            {
                if (supportedNotes.Notes.Any(
                        note => !noteInfo.Notes.Any(a => a.Denom == note.Denom && a.IsoCode == note.IsoCode)))
                {
                    notesUpdated = true;
                }
            }

            using (var transaction = _supportedNotesPersistentBlock.Transaction())
            {
                transaction.SetValue(SupportedNotesKey, supportedNotes);
                transaction.Commit();
            }

            if (notesUpdated)
            {
                SetActiveDenominations(_isoCode);
            }
        }

        private void SetActiveDenominations(string isoCode = null)
        {
            lock (_activeDenominationsLock)
            {
                var denoms = GetSupportedNotes(isoCode).ToHashSet();

                foreach (var note in GetDisabledNotes())
                {
                    denoms.Remove(note);
                }

                lock (_denomLock)
                {
                    _denominations = denoms;

                    foreach (var denom in _denominations)
                    {
                        Logger.Info($"Denomination {denom} added");
                    }
                }

                PostEvent(new NoteUpdatedEvent(NoteAcceptorId));
            }
        }

        private void PersistConfiguration()
        {
            using (var transaction = _accessor.StartTransaction())
            {
                transaction["Configuration"] = Configuration;
                transaction.Commit();
            }
        }

        private string GetConfigurationString()
        {
            return
                $"{InternalConfiguration.Model},{InternalConfiguration.Manufacturer},{InternalConfiguration.Protocol}" +
                $",{InternalConfiguration.FirmwareId},{InternalConfiguration.FirmwareRevision}";
        }

        private void ImplementationInitializationFailed(object sender, EventArgs e)
        {
            if (Implementation != null)
            {
                SetInternalConfiguration();

                Implementation.UpdateConfiguration(InternalConfiguration);
            }

            Logger.Warn("ImplementationInitializationFailed: device initialization failed");
            Disable(DisabledReasons.Device);
            Fire(NoteAcceptorLogicalStateTrigger.InspectionFailed, new InspectionFailedEvent(NoteAcceptorId));
            PostEvent(new DisabledEvent(DisabledReasons.Error));
            PostEvent(new ResetEvent(NoteAcceptorId));
        }

        private void ImplementationDisabled(object sender, EventArgs e)
        {
            Logger.Warn("ImplementationDisabled: device disabled");
        }

        private void ImplementationEnabled(object sender, EventArgs e)
        {
            Logger.Info("ImplementationEnabled: device enabled");
        }

        private void ImplementationFaultCleared(object sender, FaultEventArgs e)
        {
            foreach (NoteAcceptorFaultTypes value in Enum.GetValues(typeof(NoteAcceptorFaultTypes)))
            {
                if (!e.Fault.HasFlag(value))
                {
                    continue;
                }

                switch (value)
                {
                    case NoteAcceptorFaultTypes.FirmwareFault:
                    case NoteAcceptorFaultTypes.MechanicalFault:
                    case NoteAcceptorFaultTypes.OpticalFault:
                    case NoteAcceptorFaultTypes.ComponentFault:
                    case NoteAcceptorFaultTypes.NvmFault:
                    case NoteAcceptorFaultTypes.OtherFault:
                    case NoteAcceptorFaultTypes.StackerDisconnected:
                    case NoteAcceptorFaultTypes.StackerFull:
                    case NoteAcceptorFaultTypes.StackerJammed:
                    case NoteAcceptorFaultTypes.StackerFault:
                    case NoteAcceptorFaultTypes.NoteJammed:
                    case NoteAcceptorFaultTypes.CheatDetected:
                        if (ClearError(value))
                        {
                            Logger.Info($"ImplementationFaultCleared: REMOVED {value} from the error list.");
                            if (!AnyErrors)
                            {
                                Enable(EnabledReasons.Reset);
                            }
                        }

                        break;
                    default:
                        continue;
                }

                PostEvent(new HardwareFaultClearEvent(NoteAcceptorId, value));
            }
        }

        private void ImplementationFaultOccurred(object sender, FaultEventArgs e)
        {
            foreach (NoteAcceptorFaultTypes value in Enum.GetValues(typeof(NoteAcceptorFaultTypes)))
            {
                if (!e.Fault.HasFlag(value))
                {
                    continue;
                }

                var postFaultEvent = !ContainsError(value.ToString());

                switch (value)
                {
                    case NoteAcceptorFaultTypes.FirmwareFault:
                        // VLT-8547 JCM iVision ejects the document and doesn't report it in the ticket status
                        LastDocumentResult = DocumentResult.None;
                        HandleFault(value);
                        break;
                    case NoteAcceptorFaultTypes.OpticalFault:
                    case NoteAcceptorFaultTypes.ComponentFault:
                    case NoteAcceptorFaultTypes.NvmFault:
                    case NoteAcceptorFaultTypes.StackerFull:
                    case NoteAcceptorFaultTypes.StackerFault:
                        HandleFault(value);
                        break;
                    case NoteAcceptorFaultTypes.OtherFault:
                    case NoteAcceptorFaultTypes.MechanicalFault:
                    case NoteAcceptorFaultTypes.StackerDisconnected:
                    case NoteAcceptorFaultTypes.StackerJammed:
                    case NoteAcceptorFaultTypes.NoteJammed:
                    case NoteAcceptorFaultTypes.CheatDetected:
                        HandleFault(value);

                        // The device does not always reports a ticket status after clearing one of these faults because
                        // the document has to be manually removed to clear them.
                        if (LastDocumentResult == DocumentResult.Stacking)
                        {
                            LastDocumentResult = DocumentResult.None;
                            _stackingEventWaitHandle?.Set();
                        }

                        break;
                    default:
                        continue;
                }

                if (postFaultEvent)
                {
                    PostEvent(new HardwareFaultEvent(NoteAcceptorId, value));
                }
            }
        }

        private void HandleFault(NoteAcceptorFaultTypes value)
        {
            if (AddError(value))
            {
                Logger.Info($"ImplementationFaultOccurred: ADDED {value} to the error list.");
                Disable(DisabledReasons.Error);
            }
            else
            {
                Logger.Debug($"ImplementationFaultOccurred: DUPLICATE ERROR EVENT {value}");
            }
        }

        private void ImplementationNoteOrTicketRejected(object sender, EventArgs e)
        {
            Fire(NoteAcceptorLogicalStateTrigger.Rejected, new DocumentRejectedEvent(NoteAcceptorId));
            Logger.Warn("Note or Ticket rejected");
            LastDocumentResult = DocumentResult.Rejected;
            _stackingEventWaitHandle?.Set();
        }

        private void ImplementationNoteOrTicketReturned(object sender, EventArgs e)
        {
            Fire(NoteAcceptorLogicalStateTrigger.Returned);
            Logger.Warn("Note or Ticket returned");
            LastDocumentResult = DocumentResult.Returned;
            _stackingEventWaitHandle?.Set();
        }

        private void ImplementationNoteOrTicketRemoved(object sender, EventArgs e)
        {
            Logger.Info("ImplementationNoteOrTicketRemoved");
            // VLT-8547 JCM iVision ejects the document and doesn't report it in the ticket status
            LastDocumentResult = DocumentResult.None;
        }

        private void ImplementationNoteAccepted(object sender, NoteEventArgs e)
        {
            Fire(NoteAcceptorLogicalStateTrigger.Stacked, new CurrencyStackedEvent(NoteAcceptorId, e.Note));
            LastDocumentResult = DocumentResult.Stacked;
            _stackingEventWaitHandle?.Set();
        }

        private void ImplementationNoteReturned(object sender, NoteEventArgs e)
        {
            Fire(NoteAcceptorLogicalStateTrigger.Returned, new CurrencyReturnedEvent(NoteAcceptorId, e.Note));
            Logger.Warn($"Note returned {e.Note}");
            LastDocumentResult = DocumentResult.Returned;
            _stackingEventWaitHandle?.Set();
        }

        private void ImplementationNoteValidated(object sender, NoteEventArgs e)
        {
            var escrowEvent = new CurrencyEscrowedEvent(NoteAcceptorId, e.Note);

            if (Fire(NoteAcceptorLogicalStateTrigger.Escrowed, escrowEvent, true))
            {
                LastDocumentResult = DocumentResult.Escrowed;
                Logger.Info($"Escrowed currency {e.Note}");
            }
            else
            {
                Logger.Warn($"Escrowed currency {e.Note} from invalid state");
                Return().FireAndForget(ex => { Logger.Error($"Return: Exception occurred {ex}"); });
            }
        }

        private void ImplementationTicketStacking(object sender, EventArgs e)
        {
            Fire(NoteAcceptorLogicalStateTrigger.Stacking);
        }

        private void ImplementationTicketAccepted(object sender, TicketEventArgs e)
        {
            Fire(NoteAcceptorLogicalStateTrigger.Stacked, new VoucherStackedEvent(NoteAcceptorId, e.Barcode));
            Logger.Debug($"Voucher Accepted {e.Barcode}");
            LastDocumentResult = DocumentResult.Stacked;
            _stackingEventWaitHandle?.Set();
        }

        private void ImplementationTicketReturned(object sender, TicketEventArgs e)
        {
            Fire(NoteAcceptorLogicalStateTrigger.Returned, new VoucherReturnedEvent(NoteAcceptorId, e.Barcode));
            Logger.Warn($"Voucher returned {e.Barcode}");
            LastDocumentResult = DocumentResult.Returned;
            _stackingEventWaitHandle?.Set();
        }

        private void ImplementationTicketValidated(object sender, TicketEventArgs e)
        {
            if (Fire(
                    NoteAcceptorLogicalStateTrigger.Escrowed,
                    new VoucherEscrowedEvent(NoteAcceptorId, e.Barcode),
                    true))
            {
                LastDocumentResult = DocumentResult.Escrowed;
                Logger.Info($"Escrowed voucher {e.Barcode}");
            }
            else
            {
                Logger.Warn($"Escrowed voucher {e.Barcode} from invalid state");
                Return().FireAndForget(ex => { Logger.Error($"Return: Exception occurred {ex}"); });
            }
        }

        private async Task<bool> StackDocument(Func<Task<bool>> acceptTask)
        {
            _stackingEventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            if (await acceptTask())
            {
                ClearStackingEventWaitHandle();
                return true;
            }

            _stackingEventWaitHandle.WaitOne();
            ClearStackingEventWaitHandle();
            Logger.Debug($"StackDocument / LastDocumentResult={LastDocumentResult}");
            return LastDocumentResult == DocumentResult.Stacked;
        }

        private void ClearStackingEventWaitHandle()
        {
            _stackingEventWaitHandle?.Dispose();
            _stackingEventWaitHandle = null;
        }

        private StateMachine<NoteAcceptorLogicalState, NoteAcceptorLogicalStateTrigger> ConfigureStateMachine()
        {
            var stateMachine =
                new StateMachine<NoteAcceptorLogicalState, NoteAcceptorLogicalStateTrigger>(
                    NoteAcceptorLogicalState.Uninitialized);

            stateMachine.Configure(NoteAcceptorLogicalState.Uninitialized)
                .Permit(NoteAcceptorLogicalStateTrigger.Inspecting, NoteAcceptorLogicalState.Inspecting)
                .PermitDynamic(
                    NoteAcceptorLogicalStateTrigger.Initialized,
                    () => Enabled ? NoteAcceptorLogicalState.Idle : NoteAcceptorLogicalState.Disabled);

            stateMachine.Configure(NoteAcceptorLogicalState.Inspecting)
                .Permit(NoteAcceptorLogicalStateTrigger.InspectionFailed, NoteAcceptorLogicalState.Uninitialized)
                .PermitDynamic(
                    NoteAcceptorLogicalStateTrigger.Initialized,
                    () => Enabled ? NoteAcceptorLogicalState.Idle : NoteAcceptorLogicalState.Disabled)
                .Permit(NoteAcceptorLogicalStateTrigger.Disconnected, NoteAcceptorLogicalState.Disconnected);

            stateMachine.Configure(NoteAcceptorLogicalState.Disconnected)
                .Permit(NoteAcceptorLogicalStateTrigger.Connected, NoteAcceptorLogicalState.Inspecting);

            stateMachine.Configure(NoteAcceptorLogicalState.Idle)
                .Permit(NoteAcceptorLogicalStateTrigger.Escrowed, NoteAcceptorLogicalState.InEscrow)
                .Permit(NoteAcceptorLogicalStateTrigger.Returning, NoteAcceptorLogicalState.Returning)
                .PermitReentry(NoteAcceptorLogicalStateTrigger.Rejected)
                .Permit(NoteAcceptorLogicalStateTrigger.Disable, NoteAcceptorLogicalState.Disabled)
                .Permit(NoteAcceptorLogicalStateTrigger.Disconnected, NoteAcceptorLogicalState.Disconnected);

            stateMachine.Configure(NoteAcceptorLogicalState.InEscrow)
                .OnEntry(() => LastDocumentResult = DocumentResult.Escrowed)
                .Permit(NoteAcceptorLogicalStateTrigger.Stacking, NoteAcceptorLogicalState.Stacking)
                .Permit(NoteAcceptorLogicalStateTrigger.Returning, NoteAcceptorLogicalState.Returning)
                .Permit(NoteAcceptorLogicalStateTrigger.Disable, NoteAcceptorLogicalState.Disabled)
                .Permit(NoteAcceptorLogicalStateTrigger.Disconnected, NoteAcceptorLogicalState.Disconnected);

            stateMachine.Configure(NoteAcceptorLogicalState.Stacking)
                .OnEntry(() => LastDocumentResult = DocumentResult.Stacking)
                .Permit(NoteAcceptorLogicalStateTrigger.Stacked, NoteAcceptorLogicalState.Idle)
                .Permit(NoteAcceptorLogicalStateTrigger.Returned, NoteAcceptorLogicalState.Idle)
                .Permit(NoteAcceptorLogicalStateTrigger.Rejected, NoteAcceptorLogicalState.Idle)
                .Permit(NoteAcceptorLogicalStateTrigger.Disable, NoteAcceptorLogicalState.Disabled)
                .Permit(NoteAcceptorLogicalStateTrigger.Disconnected, NoteAcceptorLogicalState.Disconnected);

            stateMachine.Configure(NoteAcceptorLogicalState.Returning)
                .Permit(NoteAcceptorLogicalStateTrigger.Returned, NoteAcceptorLogicalState.Idle)
                .Permit(NoteAcceptorLogicalStateTrigger.Disable, NoteAcceptorLogicalState.Disabled)
                .Permit(NoteAcceptorLogicalStateTrigger.Disconnected, NoteAcceptorLogicalState.Disconnected);

            stateMachine.Configure(NoteAcceptorLogicalState.Disabled)
                .Permit(NoteAcceptorLogicalStateTrigger.Enable, NoteAcceptorLogicalState.Idle)
                .PermitReentry(NoteAcceptorLogicalStateTrigger.Disable)
                .Permit(NoteAcceptorLogicalStateTrigger.Connected, NoteAcceptorLogicalState.Inspecting)
                .Permit(NoteAcceptorLogicalStateTrigger.Disconnected, NoteAcceptorLogicalState.Disconnected);

            stateMachine.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error($"Invalid NoteAcceptor State Transition. State : {state} Trigger : {trigger}");
                });

            stateMachine.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"Transitioned From : {transition.Source} To : {transition.Destination} Trigger : {transition.Trigger}");
                });
            return stateMachine;
        }

        private bool CanFire(NoteAcceptorLogicalStateTrigger trigger)
        {
            if (!_state.CanFire(trigger))
            {
                Logger.Warn($"Cannot transition with trigger: {trigger}");
                return false;
            }

            return true;
        }

        private bool Fire(NoteAcceptorLogicalStateTrigger trigger, bool verify = false)
        {
            if (verify && !CanFire(trigger))
            {
                return false;
            }

            Logger.Debug($"Transitioning with trigger: {trigger}");
            _stateLock?.EnterWriteLock();
            try
            {
                _state.Fire(trigger);
            }
            finally
            {
                _stateLock?.ExitWriteLock();
            }

            return true;
        }

        private bool Fire<TEvent>(NoteAcceptorLogicalStateTrigger trigger, TEvent @event, bool verify = false)
            where TEvent : IEvent
        {
            if (!Fire(trigger, verify))
            {
                return false;
            }

            if (@event != null)
            {
                PostEvent(@event);
            }

            return true;
        }
    }
}