namespace Aristocrat.Monaco.Mgam.Services.CreditValidators
{
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;
    using Common.Data.Models;
    using Common.Events;
    using Kernel;
    using Newtonsoft.Json;
    using Protocol.Common.Storage.Entity;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BeginSessionWithSessionId = Commands.BeginSessionWithSessionId;
    using EndSession = Commands.EndSession;

    /// <summary>
    ///     Used to validate currency amounts requested by the EGM with the host.
    /// </summary>
    public class TransactionRetryHandler : ITransactionRetryHandler, IDisposable
    {
        private readonly ILogger<TransactionRetryHandler> _logger;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IEventBus _eventBus;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IDictionary<Type, Func<object, Task<IResponse>>> _retryCommands = new Dictionary<Type, Func<object, Task<IResponse>>>();
        private readonly IDictionary<Type, Action> _retryActions = new Dictionary<Type, Action>();
        private IList<(object request, Type type)> _transactionRequests;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ITransactionRetryHandler" /> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="unitOfWorkFactory">Unit of work factory.</param>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="commandFactory"><see cref="ICommandHandlerFactory"/>.</param>
        public TransactionRetryHandler(
            ILogger<TransactionRetryHandler> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            IEventBus eventBus,
            ICommandHandlerFactory commandFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));

            _eventBus.Subscribe<PlayEvent>(this, Handle);
            _eventBus.Subscribe<AttributesUpdatedEvent>(this, Handle);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void RegisterCommand(Type command, Func<object, Task<IResponse>> commandRetry)
        {
            _retryCommands[command] = commandRetry;
        }

        public void RegisterRetryAction(Type command, Action commandAction)
        {
            _retryActions[command] = commandAction;
        }

        public void Add(IRequest message)
        {
            if (_transactionRequests == null)
            {
                LoadTransactions();
            }

            _transactionRequests.Add((message, message.GetType()));
            Update();
        }

        public void Remove(IRequest message)
        {
            Remove((message, message.GetType()));
        }

        private async Task Handle(PlayEvent evt, CancellationToken cancellationToken)
        {
            Session session = null;
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                session = unitOfWork.Repository<Session>().Queryable().SingleOrDefault();
            }

            if (evt.SessionId == null)
            {
                if (session != null)
                {
                    await RetryTransactionRequests();
                }

                return;
            }

            var offlineVoucherPrinted = session?.OfflineVoucherPrinted ?? false;
            var barcode = session?.OfflineVoucherBarcode;

            try
            {
                try
                {
                    await _commandFactory.Execute(
                        new BeginSessionWithSessionId
                        {
                            SessionId = evt.SessionId.Value,
                            VoucherPrintedOffline = offlineVoucherPrinted,
                            Barcode = barcode
                        });
                }
                catch (ServerResponseException e) when (e.ResponseCode == ServerResponseCode.InvalidAmount)
                {
                    await EndSession();
                }

                await RetryTransactionRequests();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Begin session failure, Session ID: {evt.SessionId}, Voucher Printed: {offlineVoucherPrinted}, Barcode: {barcode}");
            }

            async Task EndSession()
            {
                try
                {
                    await _commandFactory.Execute(new EndSession());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "End Session failed.");
                }
            }
        }

        private async Task Handle(AttributesUpdatedEvent evt, CancellationToken cancellationToken)
        {
            if (CheckForSession())
            {
                return;
            }

            await RetryTransactionRequests();
        }

        private bool CheckForSession()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                return unitOfWork.Repository<Session>().Queryable().SingleOrDefault() != null;
            }
        }

        private async Task RetryTransactionRequests()
        {
            if (_transactionRequests == null)
            {
                LoadTransactions();
            }

            if (_transactionRequests.Count > 0)
            {
                foreach (var message in _transactionRequests.ToArray())
                {
                    try
                    {
                        await _retryCommands[message.type].Invoke(message.request);
                        if (_retryActions.ContainsKey(message.type))
                        {
                            _retryActions[message.type].Invoke();
                        }

                        Remove(message);
                    }
                    catch (ServerResponseException exception)
                    {
                        if (exception.ResponseCode != ServerResponseCode.ServerError)
                        {
                            Remove(message);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Failed to resend {message.type} {e}");
                    }
                }
            }
        }

        private void Remove((object, Type) message)
        {
            _transactionRequests.Remove(message);
            Update();
        }

        private void Update()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var transactionRequests = unitOfWork.Repository<TransactionRequests>().Queryable().SingleOrDefault() ??
                                          new TransactionRequests();

                transactionRequests.Requests = JsonConvert.SerializeObject(_transactionRequests);

                unitOfWork.Repository<TransactionRequests>().AddOrUpdate(transactionRequests);

                unitOfWork.SaveChanges();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void LoadTransactions()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var transactionRequests = unitOfWork.Repository<TransactionRequests>().Queryable().SingleOrDefault();
                _transactionRequests = transactionRequests == null
                    ? new List<(object, Type)>()
                    : JsonConvert.DeserializeObject<IList<(object, Type)>>(transactionRequests.Requests);
            }
        }
    }
}
