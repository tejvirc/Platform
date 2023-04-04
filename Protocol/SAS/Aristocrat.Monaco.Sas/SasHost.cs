namespace Aristocrat.Monaco.Sas
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Vouchers;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Base;
    using Common;
    using Contracts.Client;
    using Contracts.Events;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Handlers;
    using HandPay;
    using Hardware.Contracts.SerialPorts;
    using Kernel;
    using log4net;
    using Protocol.Common.Storage.Entity;
    using SasPriorityExceptionQueue = Exceptions.SasPriorityExceptionQueue;

    /// <summary>
    ///     Provides the interface between the platform and the hosts
    /// </summary>
    public class SasHost : ISasHost, IService, IPlatformCallbacks
    {
        private const int ClientNotExisting = -1;
        private const int Client1 = 0;
        private const int Client2 = 1;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private IDictionary<SasGroup, int> _sasGroupPort;
        private SasClientConfiguration _client1Configuration;
        private SasClientConfiguration _client2Configuration;

        private readonly ConcurrentDictionary<int, RunningClient> _runningClients = new ConcurrentDictionary<int, RunningClient>();
        private readonly AutoResetEvent _pendingExceptions = new AutoResetEvent(false);
        private bool _disposed;

        private IValidationHandler _validationHandler;
        private ISasTicketPrintedHandler _ticketPrintedHandler;
        private IAftRegistrationProvider _aftRegistrationProvider;
        private IReadOnlyCollection<ISasLongPollHandler> _longPollHandlers = new List<ISasLongPollHandler>();
        private ISasExceptionHandler _exceptionHandler;
        private ISasDisableProvider _disableProvider;
        private ISasHandPayCommittedHandler _sasHandPayCommittedHandler;
        private IAftTransferProvider _aftTransferProvider;
        private IPropertiesManager _propertiesManager;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IEventBus _eventBus;
        private ISerialPortsService _serialPortService;
        private ISasVoucherInProvider _sasVoucherInProvider;
        private readonly Dictionary<int, SasDiagnostics> _sasClientDiagnostics = new Dictionary<int, SasDiagnostics>();
        private readonly Dictionary<int, SasPriorityExceptionQueue> _sasExceptionQueue = new Dictionary<int, SasPriorityExceptionQueue>();

        /// <inheritdoc />
        public bool Client1HandlesGeneralControl { get; private set; }

        /// <inheritdoc />
        public void RegisterHandlers(
            ISasExceptionHandler exceptionHandler,
            IReadOnlyCollection<ISasLongPollHandler> longPollHandlers,
            IValidationHandler validationHandler,
            ISasTicketPrintedHandler ticketPrintedHandler,
            IAftRegistrationProvider aftRegistrationProvider,
            ISasHandPayCommittedHandler sasHandPayCommittedHandler,
            IAftTransferProvider aftTransferProvider,
            ISasVoucherInProvider sasVoucherInProvider)
        {
            _exceptionHandler = exceptionHandler;
            _longPollHandlers = longPollHandlers;
            _validationHandler = validationHandler;
            _ticketPrintedHandler = ticketPrintedHandler;
            _aftRegistrationProvider = aftRegistrationProvider;
            _sasHandPayCommittedHandler = sasHandPayCommittedHandler;
            _aftTransferProvider = aftTransferProvider;
            _sasVoucherInProvider = sasVoucherInProvider;
        }

        /// <inheritdoc />
        public void HandlePendingExceptions()
        {
            HandlePendingExceptionsClient(_client1Configuration);
            HandlePendingExceptionsClient(_client2Configuration);
        }

        private void HandlePendingExceptionsClient(SasClientConfiguration clientConfiguration)
        {
            var clientNumber = clientConfiguration.ClientNumber;

            _sasExceptionQueue.TryGetValue(clientNumber, out var exceptionQueue);
            if (exceptionQueue is null)
            {
                Logger.Warn($"HandlePendingExceptionsClient {clientNumber} - exceptionQueue NULL, returning...");
                return;
            }

            if (!exceptionQueue.ExceptionQueueIsEmpty)
            {
                if (string.IsNullOrEmpty(clientConfiguration.ComPort) || clientConfiguration.SasAddress == 0 ||
                    !_runningClients[clientNumber].Client.LinkUp)
                {
                    Logger.Debug($"Clearing pending exceptions for client {clientNumber}");
                    exceptionQueue.ClearPendingException();
                    return;
                }

                _pendingExceptions.Reset();
                exceptionQueue.NotifyWhenExceptionQueueIsEmpty = true;
                Logger.Debug($"Waiting on pending exceptions for client {clientNumber}");
                _pendingExceptions.WaitOne();
                exceptionQueue.NotifyWhenExceptionQueueIsEmpty = false;
                Logger.Debug($"Pending exception queue empty for client {clientNumber}");
            }
        }

        /// <inheritdoc />
        public void InjectDependencies(
            IPropertiesManager propertiesManager,
            IEventBus eventBus,
            ISerialPortsService serialPortService)
        {
            _propertiesManager = propertiesManager;
            _eventBus = eventBus;
            _serialPortService = serialPortService;

            _eventBus.Subscribe<ExceptionQueueEmptyEvent>(this, _ => _pendingExceptions.Set());
        }

        /// <inheritdoc />
        public bool IsHostOnline(SasGroup sasGroup)
        {
            var hostId = GetIdOfHostThatHandles(sasGroup);

            return hostId != ClientNotExisting && _runningClients.Count != 0 &&
                   _runningClients[hostId].Client.LinkUp;
        }

        /// <inheritdoc />
        public void Initialize(ISasDisableProvider disableProvider, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _disableProvider = disableProvider;
        }

        /// <inheritdoc />
        public void SetConfiguration(SasSystemConfiguration configuration)
        {
            Logger.Debug("SetConfiguration");

            // This method is currently called AFTER Initialize

            ConfigureSasClient(ref _client1Configuration, configuration, Client1);
            ConfigureSasClient(ref _client2Configuration, configuration, Client2);

            Client1HandlesGeneralControl = _client1Configuration.SasAddress != 0 && _client1Configuration.HandlesGeneralControl;

            // determine which client handles various real time events
            var generalControlPort = _client1Configuration.HandlesGeneralControl
                ? (_client1Configuration.SasAddress == 0 ? ClientNotExisting : Client1)
                : (_client2Configuration.SasAddress == 0 || !_client2Configuration.HandlesGeneralControl ? ClientNotExisting : Client2);
            var aftPort = _client1Configuration.HandlesAft ?
                  (_client1Configuration.SasAddress == 0 ? ClientNotExisting : Client1)
                : (_client2Configuration.SasAddress == 0 || !_client2Configuration.HandlesAft ? ClientNotExisting : Client2);
            var validationPort = _client1Configuration.HandlesValidation ?
                  (_client1Configuration.SasAddress == 0 ? ClientNotExisting : Client1)
                : (_client2Configuration.SasAddress == 0 || !_client2Configuration.HandlesValidation ? ClientNotExisting : Client2);
            var gameStartEndPort = _client1Configuration.HandlesGameStartEnd ?
                  (_client1Configuration.SasAddress == 0 ? ClientNotExisting : Client1)
                : (_client2Configuration.SasAddress == 0 || !_client2Configuration.HandlesGameStartEnd ? ClientNotExisting : Client2);
            var progressivePort = _client1Configuration.HandlesProgressives ?
                  (_client1Configuration.SasAddress == 0 ? ClientNotExisting : Client1)
                : (_client2Configuration.SasAddress == 0 || !_client2Configuration.HandlesProgressives ? ClientNotExisting : Client2);

            InitializeHostConfiguration(
                generalControlPort,
                aftPort,
                validationPort,
                gameStartEndPort,
                progressivePort);

            InitializeSasDisableManager();
        }

        private void ConfigureSasClient(ref SasClientConfiguration clientConfiguration, SasSystemConfiguration configuration, int clientNumber)
        {
            if (configuration.SasHostConfiguration.Count <= clientNumber)
            {
                return;
            }

            var hostConfiguration = configuration.SasHostConfiguration[clientNumber];
            clientConfiguration.ClientNumber = (byte)clientNumber;
            clientConfiguration.HandlesAft = configuration.SasConfiguration.System.ControlPorts.AftPort == clientNumber;
            clientConfiguration.DiscardOldestException =
                hostConfiguration.OverflowBehavior == ExceptionOverflowBehavior.DiscardOldExceptions;
            clientConfiguration.HandlesGeneralControl =
                configuration.SasConfiguration.System.ControlPorts.GeneralControlPort == clientNumber;
            clientConfiguration.HandlesLegacyBonusing =
                configuration.SasConfiguration.System.ControlPorts.LegacyBonusPort == clientNumber;
            clientConfiguration.HandlesValidation =
                configuration.SasConfiguration.System.ControlPorts.ValidationPort == clientNumber;
            clientConfiguration.LegacyHandpayReporting =
                configuration.SasConfiguration.HandPay.HandpayReportingType == SasHandpayReportingType.LegacyHandpayReporting;
            clientConfiguration.AccountingDenom = hostConfiguration.AccountingDenom;
            clientConfiguration.HandlesGameStartEnd =
                configuration.SasConfiguration.System.ControlPorts.GameStartEndHosts == (GameStartEndHost)(clientNumber + 1) ||
                configuration.SasConfiguration.System.ControlPorts.GameStartEndHosts == GameStartEndHost.Both;
            clientConfiguration.IsNoneValidation =
                configuration.SasConfiguration.System.ValidationType == SasValidationType.None;
            clientConfiguration.ComPort = _serialPortService.LogicalToPhysicalName(hostConfiguration.PortName);
            clientConfiguration.SasAddress = hostConfiguration.SasAddress;

            clientConfiguration.HandlesProgressives =
                configuration.SasConfiguration.System.ControlPorts.ProgressivePort == clientNumber;

            Logger.Debug($"Host {clientNumber} - Aft:{clientConfiguration.HandlesAft}, General:{clientConfiguration.HandlesGeneralControl}, " +
                $"Bonusing:{clientConfiguration.HandlesLegacyBonusing}, Validation:{clientConfiguration.HandlesValidation}, " +
                $"Progressives:{clientConfiguration.HandlesProgressives}, " +
                $"Game Start/End:{clientConfiguration.HandlesGameStartEnd}");
        }

        /// <inheritdoc />
        public bool StartEventSystem()
        {
            Logger.Debug("StartEventSystem: Starting Clients");
            StartSasClient(_client1Configuration);
            StartSasClient(_client2Configuration);
            return true;
        }

        private void StartSasClient(SasClientConfiguration clientConfiguration)
        {
            var clientNumber = clientConfiguration.ClientNumber;

            // only start up the client if it has a comm port defined and a valid address
            if (string.IsNullOrEmpty(clientConfiguration.ComPort) || clientConfiguration.SasAddress == 0)
            {
                return;
            }

            var exceptionQueue = new SasPriorityExceptionQueue(clientNumber, _unitOfWorkFactory, _exceptionHandler, clientConfiguration, _eventBus);
            _sasExceptionQueue.Add(clientNumber, exceptionQueue);

            var handpayQueue = new HandpayQueue(
                _unitOfWorkFactory,
                _sasHandPayCommittedHandler,
                SasConstants.SasHandpayQueueSize,
                clientNumber);

            var messageQueue = new SasPriorityMessageQueue();
            var longPollFactory = new SasLongPollParserFactory();

            var impliedAckHandler = new HostAcknowledgementProvider();

            longPollFactory.LoadParsers(clientConfiguration);

            var client = new SasClient(
                clientConfiguration,
                this,
                exceptionQueue,
                messageQueue,
                longPollFactory,
                impliedAckHandler);

            _sasClientDiagnostics.Add(clientNumber, client.Diagnostics);

            client.Initialize();
            InitializeSasLongPollHandlers(longPollFactory);
            client.AttachToCommPort(clientConfiguration.ComPort);
            var thread = new Thread(client.Run)
            {
                Name = client.GetType().ToString(),
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                Priority = ThreadPriority.Highest
            };

            thread.Start();
            _runningClients[clientNumber] = new RunningClient(client, exceptionQueue, messageQueue, handpayQueue);
            Logger.Debug($"Client{clientNumber} started");
        }

        /// <inheritdoc/>>
        public void StopEventSystem()
        {
            Logger.Debug("StopEventSystem");
            foreach (var group in Enum.GetValues(typeof(SasGroup)).Cast<SasGroup>())
            {
                _sasGroupPort[group] = ClientNotExisting;
            }

            foreach (var runningClient in _runningClients.Values.ToList())
            {
                runningClient.Dispose();
            }

            _runningClients.Clear();
        }

        /// <inheritdoc />
        public void SetLegacyBonusEnabled(bool isEnabled)
        {
            Logger.Debug($"SetLegacyBonusEnabled {isEnabled}");
        }

        /// <inheritdoc />
        public void SetAftReceiptStatus(ReceiptStatus receiptStatus)
        {
            _aftTransferProvider.CurrentTransfer.ReceiptStatus = (byte)receiptStatus;
            Logger.Debug($"SetAftReceiptStatus with status {receiptStatus.ToString()}");
        }

        /// <inheritdoc />
        public bool IsRedemptionEnabled() => _sasVoucherInProvider?.RedemptionEnabled ?? false;

        /// <inheritdoc />
        public Task<TicketInInfo> ValidateTicketInRequest(VoucherInTransaction transaction)
        {
            Logger.Debug($"ValidateTicketInRequest: with barcode {transaction.Barcode}");
            var client = GetIdOfHostThatHandles(SasGroup.Validation);

            if (client == ClientNotExisting)
            {
                _sasVoucherInProvider?.DenyTicket();
                return Task.FromResult((TicketInInfo)null);
            }

            var result =
                _sasVoucherInProvider?.ValidationTicket(transaction) ??
                Task.FromResult((TicketInInfo)null);

            return result;
        }

        /// <inheritdoc />
        public void TicketTransferComplete(AccountType accountType)
        {
            Logger.Debug("TicketTransferComplete");
            _sasVoucherInProvider.OnTicketInCompleted(accountType);
        }

        /// <inheritdoc />
        public void TicketTransferFailed(string barcode, int exceptionCode, long transactionId)
        {
            Logger.Debug("TicketTransferFailed");

            // The ticket has already been rejected prior to this call so just need to reset the status.
            _sasVoucherInProvider?.OnTicketInFailed(barcode, ((VoucherInExceptionCode)exceptionCode).ToRedemptionStatusCode(), transactionId);
        }

        /// <inheritdoc />
        public void AftTransferComplete(AftData data)
        {
            Logger.Debug($"AftTransferComplete cashable {data.CashableAmount}");
            _aftTransferProvider.UpdateFinalAftResponseData(data);
        }

        /// <inheritdoc />
        public void AftTransferFailed(AftData data, AftTransferStatusCode errorCode)
        {
            Logger.Debug($"AftTransferFailed errorCode {errorCode}");
            _aftTransferProvider.UpdateFinalAftResponseData(data, errorCode, true);
        }

        /// <inheritdoc />
        public void SetAftOutEnabled(bool isEnabled)
        {
            Logger.Debug($"SetAftOutEnabled {isEnabled}");
        }

        /// <inheritdoc />
        public void SetAftInEnabled(bool isEnabled)
        {
            Logger.Debug($"SetAftInEnabled {isEnabled}");
        }

        /// <inheritdoc />
        public void AftBonusAwarded(AftData data)
        {
            Logger.Debug($"AftBonusAwarded cashable {data.CashableAmount}");
        }

        /// <inheritdoc />
        public bool CanValidateTicketOutRequest(ulong amount, TicketType ticketType)
        {
            var canValidateTicketOutRequest = _validationHandler?.CanValidateTicketOutRequest(amount, ticketType) ?? false;
            Logger.Debug($"CanValidateTicketOutRequest amount={amount} ticketType={ticketType} canValidate={canValidateTicketOutRequest}");
            return canValidateTicketOutRequest;
        }

        /// <inheritdoc />
        public Task<TicketOutInfo> ValidateTicketOutRequest(ulong amount, TicketType ticketType)
        {
            Logger.Debug($"ValidateTicketOutRequest Amount {amount}, type {ticketType}");
            return _validationHandler.HandleTicketOutValidation(amount, ticketType);
        }

        /// <inheritdoc />
        public Task<TicketOutInfo> ValidateHandpayRequest(ulong amount, HandPayType type)
        {
            Logger.Debug($"ValidateTicketOutRequest Amount {amount}, type {type}");
            return _validationHandler.HandleHandPayValidation(amount, type);
        }

        /// <inheritdoc />
        public void VoucherOutCanceled()
        {
            Logger.Debug("VoucherOutCanceled");
        }

        /// <inheritdoc />
        public void TicketPrinted()
        {
            Logger.Debug("TicketPrinted");
            _ticketPrintedHandler.ProcessPendingTickets();
        }

        /// <inheritdoc />
        public void HandPayValidated()
        {
            Logger.Debug("HandPayValidated");
            _ticketPrintedHandler.ProcessPendingTickets();
        }

        /// <inheritdoc />
        public void RomSignatureCalculated(ushort signature, byte clientNumber)
        {
            Logger.Debug($"RomSignatureCalculated {signature} for client {clientNumber}");
            if (clientNumber < _runningClients.Count)
            {
                var address = _runningClients[clientNumber].Client.Configuration.SasAddress;
                var queue = _runningClients[clientNumber].MessageQueue;
                var response =
                    new RomSignatureVerificationResponse(address, signature);
                queue.QueueMessage(response);
            }
        }

        /// <inheritdoc />
        public void AftLockCompleted()
        {
            Logger.Debug("AftLockCompleted");
        }

        /// <inheritdoc />
        public SasDiagnostics GetSasClientDiagnostics(int clientNumber)
        {
            _sasClientDiagnostics.TryGetValue(clientNumber, out var diagnostics);
            return diagnostics;
        }

        /// <inheritdoc />
        public string Name => "SasHost";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new List<Type> { typeof(ISasClient) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <summary>
        ///     Dispose of resources
        /// </summary>
        /// <param name="disposing">True if disposing the first time</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _pendingExceptions.Dispose();

                // make sure all the sas client threads are stopped
                StopEventSystem();
            }

            _disposed = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void LinkUp(bool linkUp, int host)
        {
            var linkState = linkUp ? "Up" : "Down";

            var disableOnCommunicationsLostEnable = _propertiesManager.GetValue(
                SasProperties.SasFeatureSettings,
                new SasFeatures()).DisableOnDisconnect;

            // If EGM shouldn't be disabled on comms timeout or Not a General Control Port
            var generalControlHost = GetIdOfHostThatHandles(SasGroup.GeneralControl);

            // turn off autoplay if the general control host goes down
            if (generalControlHost == host && !linkUp)
            {
                Logger.Debug($"turning off auto play due to host {host} being down");
                _eventBus.Publish(new AutoPlayRequestedEvent(false));
            }

            switch (host)
            {
                case Client1 when linkUp:
                    _eventBus.Publish(new HostOnlineEvent(1, IsProgressiveHost(Client1)));
                    break;
                case Client1:
                    _eventBus.Publish(new HostOfflineEvent(1, IsProgressiveHost(Client1)));
                    break;
                case Client2 when linkUp:
                    _eventBus.Publish(new HostOnlineEvent(2, IsProgressiveHost(Client2)));
                    break;
                case Client2:
                    _eventBus.Publish(new HostOfflineEvent(2, IsProgressiveHost(Client2)));
                    break;
            }

            Logger.Debug($"link state is {linkState} for host {host}");
            var state = BaseConstants.HostDisableStates[host];
            if (linkUp)
            {
                _disableProvider.Enable(state).FireAndForget();
            }
            else
            {
                _disableProvider.Disable(
                    SystemDisablePriority.Normal,
                    state,
                    disableOnCommunicationsLostEnable && host == generalControlHost).FireAndForget();

                if (host == GetIdOfHostThatHandles(SasGroup.Aft))
                {
                    // Cancel any pending AFT Registration cycle
                    _aftRegistrationProvider.AftRegistrationCycleInterrupted();
                }
            }
        }

        /// <inheritdoc />
        public void ToggleCommunicationsEnabled(bool enabled, byte host)
        {
            Logger.Debug($"ToggleCommunicationEnabled host {host}, enabled {enabled}");
            if (enabled)
            {
                _serialPortService.RegisterPort(host == Client1 ? _client1Configuration.ComPort : _client2Configuration.ComPort);
            }
            else
            {
                _serialPortService.UnRegisterPort(host == Client1 ? _client1Configuration.ComPort : _client2Configuration.ComPort);
            }
        }

        private void InitializeSasLongPollHandlers(ISasParserFactory factory)
        {
            foreach (var handler in _longPollHandlers)
            {
                foreach (var command in handler.Commands)
                {
                    factory.InjectHandler(handler, command);
                }
            }
        }

        private void InitializeHostConfiguration(
            int generalControlPort,
            int aftPort,
            int validationPort,
            int gameStartEndPort,
            int progressivePort)
        {
            _sasGroupPort = new Dictionary<SasGroup, int>
            {
                { SasGroup.Aft, aftPort },
                { SasGroup.GeneralControl, generalControlPort },
                { SasGroup.Validation, validationPort },
                { SasGroup.GameStartEnd, gameStartEndPort },
                { SasGroup.Progressives, progressivePort }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public int GetIdOfHostThatHandles(SasGroup sasGroup)
        {
            return _sasGroupPort != null && _sasGroupPort.TryGetValue(sasGroup, out var hostId)
                ? hostId
                : ClientNotExisting;
        }

        private void InitializeSasDisableManager()
        {
            var sasSettings = _propertiesManager
                .GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            // Start in a disabled state until the Sas host connects.
            var egmDisabledOnCommsTimeoutEnabled = sasSettings.DisableOnDisconnect;

            if (egmDisabledOnCommsTimeoutEnabled)
            {
                _disableProvider.Disable(
                        SystemDisablePriority.Normal,
                        BaseConstants.HostDisableStates[GetIdOfHostThatHandles(SasGroup.GeneralControl)])
                    .WaitForCompletion();
            }

            if (sasSettings.DisabledOnPowerUp)
            {
                var state = Client1HandlesGeneralControl
                    ? DisableState.PowerUpDisabledByHost0
                    : DisableState.PowerUpDisabledByHost1;
                _disableProvider.Disable(SystemDisablePriority.Normal, state).WaitForCompletion();
            }
        }

        private bool IsProgressiveHost(int hostId)
        {
            return hostId == GetIdOfHostThatHandles(SasGroup.Progressives);
        }
    }
}