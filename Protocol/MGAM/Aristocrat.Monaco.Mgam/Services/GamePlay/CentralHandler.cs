namespace Aristocrat.Monaco.Mgam.Services.GamePlay
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using CreditValidators;
    using Attributes;
    using Commands;
    using Common.Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Kernel;
    using RequestPlay = Commands.RequestPlay;

    /// <summary>
    ///     A <see cref="ICentralHandler" /> implementation
    /// </summary>
    public class CentralHandler : ICentralHandler, IDisposable
    {
        private const int MaxTimeout = 60000;
        private readonly IBank _bank;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly ICentralProvider _centralProvider;
        private readonly ILogger<CentralHandler> _logger;
        private readonly IEventBus _bus;
        private readonly IGameProvider _gameProvider;
        private readonly IAttributeManager _attributes;
        private readonly EventWaitHandle _readyToPlayWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly EventWaitHandle _recoveryWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CentralHandler" /> class.
        /// </summary>
        public CentralHandler(
            ICentralProvider centralProvider,
            ICommandHandlerFactory commandFactory,
            IBank bank, ILogger<CentralHandler> logger,
            IEventBus bus,
            IGameProvider gameProvider,
            IAttributeManager attributes,
            ITransactionRetryHandler retryHandler)
        {
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));

            if (retryHandler == null)
            {
                throw new ArgumentNullException(nameof(retryHandler));
            }

            retryHandler.RegisterRetryAction(
                typeof(Aristocrat.Mgam.Client.Messaging.RequestPlay),
                () => _recoveryWaitHandle.Set());

            _centralProvider.Register(this, ProtocolNames.MGAM);
            
            var history = _centralProvider.Transactions
                .Where(t => t.OutcomeState == OutcomeState.Committed)
                .OrderBy(h => h.TransactionId).ToList();
            foreach (var log in history)
            {
                _centralProvider.AcknowledgeOutcome(log.TransactionId);
            }

            _bus.Subscribe<OutcomeReceivedEvent>(this,
                evt =>
                {
                    // The transaction must be acknowledged, but this doesn't mean anything to the MGAM host
                    _centralProvider.AcknowledgeOutcome(evt.Transaction.TransactionId);
                });

            _bus.Subscribe<OutcomeFailedEvent>(this,
                evt =>
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                });

            _bus.Subscribe<AttributeChangedEvent>(this, Handle, evt => evt.AttributeName.Equals(AttributeNames.ConnectionTimeout));
            _bus.Subscribe<AttributesUpdatedEvent>(this, Handle);
            _bus.Subscribe<PlayEvent>(this,
                _ =>
                {
                    _readyToPlayWaitHandle.Set();
                });
            _bus.Subscribe<HostOfflineEvent>(this,
                _ =>
                {
                    _readyToPlayWaitHandle.Reset();
                });
        }

        /// <inheritdoc />
        public async Task RequestOutcomes(CentralTransaction transaction, bool isRecovering = false)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            _readyToPlayWaitHandle.WaitOne(MaxTimeout);

            if(isRecovering)
            {
                if(_recoveryWaitHandle.WaitOne(MaxTimeout))
                {
                    _recoveryWaitHandle.Reset();
                    return;
                }
            }

            _logger.LogInfo($"Requesting outcomes for {transaction}");

            try
            {
                var currentGame = _gameProvider.GetGame(transaction.GameId);

                await _commandFactory.Execute(
                    new RequestPlay(
                        (int)_bank.QueryBalance(AccountType.Cashable).MillicentsToCents(),
                        (int)_bank.QueryBalance(AccountType.Promo).MillicentsToCents(),
                        Convert.ToInt32(transaction.TemplateId),
                        (int)(transaction.WagerAmount / transaction.Denomination),
                        (int)transaction.Denomination.MillicentsToCents(),
                        (int)(currentGame.ProductCode ?? transaction.GameId),
                        transaction.TransactionId,
                        _cancellationTokenSource.Token));

            }
            catch (ServerResponseException response)
            {
                _logger.LogError($"Begin Session failed ResponseCode:{response.ResponseCode}");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _centralProvider.Clear(ProtocolNames.MGAM);

                _bus.UnsubscribeAll(this);

                _cancellationTokenSource.Dispose();
                _readyToPlayWaitHandle.Dispose();
                _recoveryWaitHandle.Dispose();
            }

            _disposed = true;
        }

        private void Handle(AttributesUpdatedEvent evt)
        {
            SetRequestTimeout();
        }

        private void Handle(AttributeChangedEvent evt)
        {
            SetRequestTimeout();
        }

        private void SetRequestTimeout()
        {
            _ = _attributes.Get(
                AttributeNames.ConnectionTimeout,
                ProtocolConstants.DefaultConnectionTimeout);
        }
    }
}