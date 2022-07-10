namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Client.Messages;
    using Client.WorkFlow;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Service responsible for monitoring CreditIn events in platform and send appropriate commands to CentralServer.
    ///     Includes cash, tickets and AFT.
    /// </summary>
    public class CreditInService : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly ICentralManager _centralManager;
        private readonly IPlayerSessionService _playerSessionService;
        private readonly IBank _bank;
        private readonly ITransactionIdProvider _transactionIdProvider;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameProvider _gameProvider;
        private readonly IGameDataService _gameDataService;

        private bool _disposed;

        /// <summary>
        ///     Constructor to initialize CreditInService where it will subscribe for CreditIn related events and send messages to
        ///     CentralServer.
        /// </summary>
        /// <param name="eventBus">EventBus to subscribe events from platform.</param>
        /// <param name="centralManager">ICentralManager implementation, to send messages to central server.</param>
        /// <param name="playerSessionService">The player session service</param>
        /// <param name="propertiesManager">The property manager</param>
        /// <param name="transactionIdProvider">The transaction ID provider history</param>
        /// <param name="bank">The bank</param>
        /// <param name="gameProvider">The game provider service</param>
        /// <param name="gameDataService">The game data service</param>
        public CreditInService(
            IEventBus eventBus,
            ICentralManager centralManager,
            IPlayerSessionService playerSessionService,
            IPropertiesManager propertiesManager,
            ITransactionIdProvider transactionIdProvider,
            IBank bank,
            IGameProvider gameProvider,
            IGameDataService gameDataService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _centralManager = centralManager ?? throw new ArgumentNullException(nameof(centralManager));
            _playerSessionService =
                playerSessionService ?? throw new ArgumentNullException(nameof(playerSessionService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactionIdProvider = transactionIdProvider ?? throw new ArgumentNullException(nameof(transactionIdProvider));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));

            SubscribeEvents();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<CurrencyInCompletedEvent>(this, Handle);
            _eventBus.Subscribe<VoucherRedeemedEvent>(this, Handle);
            _eventBus.Subscribe<WatOnCompleteEvent>(this, Handle);
        }

        private async void Handle(CurrencyInCompletedEvent evt)
        {
            if (evt.Amount == 0)
            {
                return;
            }

#if !RETAIL
            if (evt.Amount % 100000 != 0) // Whole dollars only!
            {
                return;
            }
#endif

            var transactionInfo = evt.Transaction == null
                ? "null"
                : "Id=" + evt.Transaction.TransactionId + ", Amount= " + evt.Transaction.Amount;

            try
            {
                Logger.Debug($"Notify Credit In transaction : [{transactionInfo}], Amount={evt.Amount}");

                await TrySend(CommandTransactionType.BillInserted, evt.Amount);
            }
            catch (UnexpectedResponseException respEx)
            {
                Logger.Error($"Failed to send credit in message : [{transactionInfo}], Amount={evt.Amount}", respEx);

                HandleResponseError(respEx);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(
                    $"No player ID available for credit in message : [{transactionInfo}], Amount={evt.Amount}",
                    ex);

                HandleResponseError(null);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to send credit in message : [{transactionInfo}], Amount={evt.Amount}", ex);

                HandleResponseError(null);
            }
        }

        private async void Handle(VoucherRedeemedEvent evt)
        {
            if (evt.Transaction.Amount == 0)
            {
                return;
            }

            var transType = CommandTransactionType.Unknown;

            switch (evt.Transaction.TypeOfAccount)
            {
                case AccountType.Cashable:
                    transType = CommandTransactionType.TicketInserted;
                    break;
                case AccountType.NonCash:
                    transType = CommandTransactionType.TicketInsertedNonCashable;
                    break;
                case AccountType.Promo:
                    transType = CommandTransactionType.TicketInsertedPromo;
                    break;
            }

            try
            {
                Logger.Debug(
                    $"Notify Voucher In transaction : ({evt.Transaction.TransactionId}, TransType={transType}, Amount={evt.Transaction.Amount})");

                await TrySend(transType, evt.Transaction.Amount);
            }
            catch (UnexpectedResponseException respEx)
            {
                Logger.Error(
                    $"Failed to send voucher in message : (TransType{transType}, Amount={evt.Transaction.Amount})",
                    respEx);

                HandleResponseError(respEx);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(
                    $"No player ID available for voucher in message : (TransType{transType}, Amount={evt.Transaction.Amount})",
                    ex);

                HandleResponseError(null);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Failed to send voucher in message : (TransType{transType}, Amount={evt.Transaction.Amount})",
                    ex);
                HandleResponseError(null);
            }
        }

        private async void Handle(WatOnCompleteEvent evt)
        {
            if (evt.Transaction.TransactionAmount == 0)
            {
                return;
            }

            Logger.Debug($"Notify Aft In transaction : ({evt.Transaction.TransactionId})");

            await TrySend(CommandTransactionType.AftInNonCashable, evt.Transaction.TransferredNonCashAmount);
            await TrySend(CommandTransactionType.AftInPromo, evt.Transaction.TransferredPromoAmount);
            await TrySend(CommandTransactionType.AftInCashable, evt.Transaction.TransferredCashableAmount);
        }

        private async Task<TransactionRequest> CreateTransactionRequest(CommandTransactionType aftInType, long amount)
        {
            var gameDetail = _gameProvider.GetActiveGame();

            return new TransactionRequest
            {
                TransactionId = (uint) _transactionIdProvider.GetNextTransactionId(),
                TransactionType = aftInType,
                RequestTimeout =
                    new LockupRequestTimeout
                    {
                        LockupKey = HhrConstants.CreditInCmdTransactionErrorKey,
                        LockupString = Localizer.For(CultureFor.Operator)
                            .GetString(ResourceKeys.CreditInCmdFailedMsg),
                        LockupHelpText = Localizer.For(CultureFor.Operator)
                            .GetString(ResourceKeys.CreditInCmdFailedHelpMsg)
                    },
                PlayerId = await _playerSessionService.GetCurrentPlayerId(),
                Credit = (uint)amount.MillicentsToCents(),
                Debit = 0,
                LastGamePlayTime = _propertiesManager.GetValue(HHRPropertyNames.LastGamePlayTime, 0u),
                GameMapId = await _gameDataService.GetDefaultGameMapIdAsync(),
                Flags = 0,
                Denomination = (uint)gameDetail.denomination.Value.MillicentsToCents(),
                HandpayType = (uint)HandpayType.HandpayTypeNone,
                TimeoutInMilliseconds = HhrConstants.MsgTransactionTimeoutMs,
                RetryCount = HhrConstants.RetryCount,
                CashBalance =
                    (uint)(_bank.QueryBalance(AccountType.Cashable) + (uint)_bank.QueryBalance(AccountType.Promo))
                    .MillicentsToCents(),
                NonCashBalance = (uint)_bank.QueryBalance(AccountType.NonCash).MillicentsToCents()
            };
        }

        private void HandleResponseError(UnexpectedResponseException respEx)
        {
            if (respEx != null && respEx.Response is CloseTranErrorResponse transErrorResponse)
            {
                Logger.Error($"Error Response : ({transErrorResponse.ErrorCode} - {transErrorResponse.ErrorText})");
            }
        }

        private async Task TrySend(CommandTransactionType aftTransType, long amount)
        {
            if (amount == 0)
            {
                return;
            }

            try
            {
                await _centralManager.Send<TransactionRequest, CloseTranResponse>(
                    await CreateTransactionRequest(
                        aftTransType,
                        amount));
            }
            catch (UnexpectedResponseException respEx)
            {
                Logger.Error(
                    $"Failed to notify Aft In transaction - Unexpected response : (TransType{aftTransType}, Amount={amount})",
                    respEx);

                HandleResponseError(respEx);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(
                    $"No player ID available for Aft In transaction : (TransType{aftTransType}, Amount={amount})",
                    ex);

                HandleResponseError(null);
            }
            catch (Exception ex)
            {
                Logger.Debug($"Failed to notify Aft In transaction : (TransType{aftTransType}, Amount={amount})", ex);

                HandleResponseError(null);
            }
        }
    }
}