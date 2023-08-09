namespace Aristocrat.Monaco.Hardware.Fake
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Common.Currency;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.IO;
    using Contracts.NoteAcceptor;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;

    [HardwareDevice("Fake", DeviceType.NoteAcceptor)]
    public class FakeNoteAcceptorAdapter : INoteAcceptorImplementation
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly TimeSpan ReadingDelay = TimeSpan.FromMilliseconds(500);
        private static readonly Dictionary<string, List<int>> SupportedCurrencies = new();

        private readonly IEventBus _eventBus;

        private readonly List<int> _standardDenominations = new()
        {
            1,
            2,
            5,
            10,
            20,
            50,
            100
        };

        private readonly Dictionary<int, Note> _noteTable = new();
        private bool _disposed;

        /// <summary>
        ///     Construct a <see cref="FakeNoteAcceptorAdapter" />
        ///     <param name="eventBus">The device option configuration values.</param>
        /// </summary>
        public FakeNoteAcceptorAdapter(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            Logger.Debug("Constructed");
        }

        public bool IsOpen { get; set; }

        /// <summary>
        ///     Releases the unmanaged resources used by the Aristocrat.Monaco.Hardware.Fake.FakeNoteAcceptorAdapter and optionally
        ///     releases the managed resources.
        /// </summary>
        /// resources.
        /// </param>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool IsConnected { get; set; }

        /// <inheritdoc />
        public bool IsInitialized { get; set; }

        /// <inheritdoc />
        public bool IsEnabled { get; set; }

        /// <inheritdoc />
        public int Crc => 0xFFFF;

        /// <inheritdoc />
        public string Protocol => "Fake";

        /// <inheritdoc />
        public event EventHandler<EventArgs> Initialized;

        /// <inheritdoc />
        public event EventHandler<EventArgs> InitializationFailed;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Enabled;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Disabled;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Connected;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Disconnected;

        /// <inheritdoc />
        public event EventHandler<EventArgs> ResetSucceeded;

        /// <inheritdoc />
        public event EventHandler<EventArgs> ResetFailed;

        /// <inheritdoc />
        public bool Open()
        {
            if (IsOpen)
            {
                return true;
            }

            GetSupportedCurrencies();

            _noteTable.Clear();
            var noteId = 1;

            foreach (var currency in SupportedCurrencies)
            {
                foreach (var value in currency.Value)
                {
                    var note = new Note
                    {
                        NoteId = noteId++, Value = value, ISOCurrencySymbol = currency.Key, Version = 1
                    };

                    _noteTable[note.NoteId] = note;
                }
            }

            //prevent a double subscription for the FakeDeviceConnectedEvent in the case of Open->Close->Open
            _eventBus?.UnsubscribeAll(this);

            _eventBus?.Subscribe<FakeNoteAcceptorEvent>(this, HandleEvent);
            _eventBus?.Subscribe<FakeStackerEvent>(this, HandleEvent);

            ////We assume the device will be opened by default
            _eventBus?.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);
            IsConnected = true;
            OnConnected();

            return true;
        }

        /// <inheritdoc />
        public bool Close()
        {
            _eventBus?.UnsubscribeAll(this);
            _eventBus?.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);

            return true;
        }

        /// <inheritdoc />
        public Task<bool> Enable()
        {
            IsEnabled = true;
            Logger.Info("Enabled fake note acceptor adapter");
            OnEnabled(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Disable()
        {
            IsEnabled = false;
            Logger.Warn("Disabled Fake note acceptor adapter.");
            OnDisabled(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SelfTest(bool nvm)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<int> CalculateCrc(int seed)
        {
            return Task.FromResult(Crc);
        }

        /// <inheritdoc />
        public void UpdateConfiguration(IDeviceConfiguration internalConfiguration)
        {
        }

        /// <inheritdoc />
        public int VendorId => 0;

        /// <inheritdoc />
        public int ProductId => 0;

        /// <inheritdoc />
        public bool IsDfuCapable => false;

        /// <inheritdoc />
        public bool IsDfuInProgress => false;

        /// <inheritdoc />
        public event EventHandler<ProgressEventArgs> DownloadProgressed;

        /// <inheritdoc />
        public Task<bool> Initialize()
        {
            return Initialize(null);
        }

        /// <inheritdoc />
        public Task<bool> Initialize(ICommunicator communicator)
        {
            Open();
            IsInitialized = true;
            Logger.Info("Initialized fake note acceptor adapter");
            OnInitialized(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Detach()
        {
            IsConnected = false;
            IsOpen = false;
            Logger.Info("Connected fake note acceptor adapter");
            OnDisconnected();
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Reconnect()
        {
            Detach();
            Open();
            IsConnected = true;
            Logger.Info("Enabled fake note acceptor adapter");
            OnConnected();
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<DfuStatus> Download(Stream firmware)
        {
            return Task.FromResult(DfuStatus.ErrUnknown);
        }

        /// <inheritdoc />
        public Task<DfuStatus> Upload(Stream firmware)
        {
            return Task.FromResult(DfuStatus.ErrUnknown);
        }

        /// <inheritdoc />
        public void Abort()
        {
        }

        public Task<bool> Initialize(IGdsCommunicator communicator)
        {
            return Initialize(communicator as ICommunicator);
        }

        /// <inheritdoc />
        public NoteAcceptorFaultTypes Faults { get; set; }

        public IEnumerable<INote> SupportedNotes => _noteTable.Values.ToArray();

        public IComConfiguration LastComConfiguration { get; set; }

        /// <inheritdoc />
        public string Manufacturer => "Fake Note Acceptor";

        /// <inheritdoc />
        public string Model => "FakeModel";

        /// <inheritdoc />
        public string FirmwareId => "1";

        /// <inheritdoc />
        public string FirmwareRevision => "2";

        /// <inheritdoc />
        public string SerialNumber => "3";

        /// <inheritdoc />
        public event EventHandler<FaultEventArgs> FaultCleared;

        /// <inheritdoc />
        public event EventHandler<FaultEventArgs> FaultOccurred;

        /// <inheritdoc />
        public event EventHandler<NoteEventArgs> NoteAccepted;

        /// <inheritdoc />
        public event EventHandler<NoteEventArgs> NoteReturned;

        /// <inheritdoc />
        public event EventHandler<NoteEventArgs> NoteValidated;

        /// <inheritdoc />
        public event EventHandler NoteOrTicketStacking;

        /// <inheritdoc />
        public event EventHandler<TicketEventArgs> TicketAccepted;

        /// <inheritdoc />
        public event EventHandler<TicketEventArgs> TicketReturned;

        /// <inheritdoc />
        public event EventHandler<TicketEventArgs> TicketValidated;

        /// <inheritdoc />
        public event EventHandler<EventArgs> NoteOrTicketRejected;

        /// <inheritdoc />
        public event EventHandler<EventArgs> NoteOrTicketRemoved;

        /// <inheritdoc />
        public event EventHandler<EventArgs> UnknownDocumentReturned;

        /// <inheritdoc />
        public async Task<bool> AcceptNote()
        {
            Logger.Info("Accepted note with fake note acceptor adapter");
            await Task.Delay(ReadingDelay);
            OnNoteAccepted(new NoteEventArgs());
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> AcceptTicket()
        {
            Logger.Info("Accepted ticket with fake note acceptor adapter");
            await Task.Delay(ReadingDelay);
            OnTicketAccepted(new TicketEventArgs { Barcode = "" });
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> Return()
        {
            Logger.Info("Returned for fake note acceptor adapter");
            await Task.Delay(ReadingDelay);
            OnNoteReturned(new NoteEventArgs());
            return true;
        }

        /// <inheritdoc />
        public Task<string> ReadMetrics()
        {
            Logger.Info("Read metrics for fake note acceptor adapter");
            return Task.FromResult(string.Empty);
        }

        public Task<bool> ReadNoteTable()
        {
            Logger.Info("Read note table fake note acceptor adapter");
            return Task.FromResult(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        /// <summary>Handle a <see cref="FakeStackerEvent" />.</summary>
        /// <param name="fakeStackerEvent">The <see cref="FakeStackerEvent" /> to handle.</param>
        protected virtual void HandleEvent(FakeStackerEvent fakeStackerEvent)
        {
            HandleFaultChanged(fakeStackerEvent.Jam, NoteAcceptorFaultTypes.NoteJammed);
            HandleFaultChanged(fakeStackerEvent.Fault, NoteAcceptorFaultTypes.StackerFault);
            HandleFaultChanged(fakeStackerEvent.Full, NoteAcceptorFaultTypes.StackerFull);
            HandleFaultChanged(fakeStackerEvent.Disconnect, NoteAcceptorFaultTypes.StackerDisconnected);
        }

        /// <summary>Handle a <see cref="FakeNoteAcceptorEvent" />.</summary>
        /// <param name="fakeNoteAcceptorEvent">The <see cref="FakeNoteAcceptorEvent" /> to handle.</param>
        protected void HandleEvent(FakeNoteAcceptorEvent fakeNoteAcceptorEvent)
        {
            HandleFaultChanged(fakeNoteAcceptorEvent.Jam, NoteAcceptorFaultTypes.NoteJammed);
            HandleFaultChanged(fakeNoteAcceptorEvent.Cheat, NoteAcceptorFaultTypes.CheatDetected);
        }

        /// <summary>Handle a <see cref="FakeDeviceConnectedEvent" />.</summary>
        /// <param name="fakeDeviceConnectedEvent">The <see cref="FakeDeviceConnectedEvent" /> to handle.</param>
        protected void HandleEvent(FakeDeviceConnectedEvent fakeDeviceConnectedEvent)
        {
            if (fakeDeviceConnectedEvent.Type != DeviceType.NoteAcceptor)
            {
                return;
            }

            if (!fakeDeviceConnectedEvent.Connected)
            {
                IsOpen = false;
                Logger.Debug("Closed fake note acceptor adapter port");
            }
            else
            {
                IsOpen = true;
                Logger.Debug("Opened fake note acceptor adapter port");
            }
        }

        /// <summary>Executes the <see cref="Initialized" /> action.</summary>
        protected void OnInitialized(EventArgs e)
        {
            Initialized?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="InitializationFailed" /> action.</summary>
        protected void OnInitializationFailed(EventArgs e)
        {
            InitializationFailed?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="Enabled" /> action.</summary>
        protected void OnEnabled(EventArgs e)
        {
            Enabled?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="Disable" /> action.</summary>
        protected void OnDisabled(EventArgs e)
        {
            Disabled?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="Connected" /> action.</summary>
        protected void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="Disconnected" /> action.</summary>
        protected void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="ResetSucceeded" /> action.</summary>
        protected void OnResetSucceeded()
        {
            ResetSucceeded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="ResetFailed" /> action.</summary>
        protected void OnResetFailed()
        {
            ResetFailed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="DownloadProgressed" /> action.</summary>
        protected void OnDownloadProgressed()
        {
            DownloadProgressed?.Invoke(this, (ProgressEventArgs)EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="FaultCleared" /> action.</summary>
        protected void OnFaultCleared(FaultEventArgs e)
        {
            FaultCleared?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnFaultOccurred" /> action.</summary>
        protected void OnFaultOccurred(FaultEventArgs e)
        {
            FaultOccurred?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnNoteAccepted" /> action.</summary>
        protected void OnNoteAccepted(NoteEventArgs e)
        {
            NoteAccepted?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnNoteReturned" /> action.</summary>
        protected void OnNoteReturned(NoteEventArgs e)
        {
            NoteReturned?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnNoteValidated" /> action.</summary>
        protected void OnNoteValidated(NoteEventArgs e)
        {
            NoteValidated?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnTicketAccepted" /> action.</summary>
        protected void OnTicketAccepted(TicketEventArgs e)
        {
            TicketAccepted?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnTicketReturned" /> action.</summary>
        protected void OnTicketReturned(TicketEventArgs e)
        {
            TicketReturned?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnTicketValidated" /> action.</summary>
        protected void OnTicketValidated(TicketEventArgs e)
        {
            TicketValidated?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnNoteOrTicketRejected" /> action.</summary>
        protected void OnNoteOrTicketRejected(EventArgs e)
        {
            NoteOrTicketRejected?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnNoteOrTicketRemoved" /> action.</summary>
        protected void OnNoteOrTicketRemoved(EventArgs e)
        {
            NoteOrTicketRemoved?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnNoteOrTicketStacking" /> action.</summary>
        protected void OnNoteOrTicketStacking()
        {
            NoteOrTicketStacking?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="OnUnknownDocumentReturned" /> action.</summary>
        protected void OnUnknownDocumentReturned()
        {
            UnknownDocumentReturned?.Invoke(this, EventArgs.Empty);
        }

        private void HandleFaultChanged(bool state, NoteAcceptorFaultTypes fault)
        {
            if (state && (Faults & fault) == 0)
            {
                Faults |= fault;
                FaultOccurred?.Invoke(this, new FaultEventArgs { Fault = fault });
            }
            else if (!state && (Faults & fault) != 0)
            {
                Faults &= ~fault;
                FaultCleared?.Invoke(this, new FaultEventArgs { Fault = fault });
            }
        }

        private void GetSupportedCurrencies()
        {
            var currencies = CurrencyLoader.GetCurrenciesFromWindows(Logger);

            foreach (var currency in currencies.Keys)
            {
                if (Enum.IsDefined(typeof(ISOCurrencyCode), currency.ToUpper()) &&
                    !SupportedCurrencies.ContainsKey(currency))
                {
                    SupportedCurrencies[currency] = _standardDenominations;
                }
            }
        }
    }
}