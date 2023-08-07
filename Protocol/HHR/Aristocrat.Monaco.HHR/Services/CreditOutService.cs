namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Client.Messages;
    using Client.WorkFlow;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using HHRHandpayType = Client.Messages.HandpayType;

    /// <summary>
    ///     Service responsible for monitoring CreditOut events in platform and send appropriate commands to CentralServer.
    ///     Includes cash, tickets and AFT.
    /// </summary>
    public class CreditOutService : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IBank _bank;
        private readonly ICentralManager _centralManager;
        private readonly IEventBus _eventBus;
        private readonly ITransactionIdProvider _transactionIdProvider;
        private readonly IPlayerSessionService _playerSessionService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameDataService _gameDataService;
        private bool _disposed;

        /// <summary>
        ///     Constructor to initialize CreditOutService where it will subscribe for CreditOut related events and send messages
        ///     to CentralServer.
        /// </summary>
        /// <param name="eventBus">EventBus to subscribe events from platform.</param>
        /// <param name="centralManager">ICentralManager implementation, to send messages to central server.</param>
        /// <param name="playerSessionService">The Player Session Service</param>
        /// <param name="transactionIdProvider">The transaction Id Provider Service</param>
        /// <param name="propertiesManager">The Properties Manager Service</param>
        /// <param name="bank">The Bank service</param>
        /// <param name="gameDataService">The game data service</param>
        public CreditOutService(
            IEventBus eventBus,
            ICentralManager centralManager,
            IPlayerSessionService playerSessionService,
            ITransactionIdProvider transactionIdProvider,
            IPropertiesManager propertiesManager,
            IBank bank,
            IGameDataService gameDataService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _centralManager = centralManager ?? throw new ArgumentNullException(nameof(centralManager));
            _playerSessionService = playerSessionService ?? throw new ArgumentNullException(nameof(playerSessionService));
            _transactionIdProvider = transactionIdProvider ?? throw new ArgumentNullException(nameof(transactionIdProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
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

        private async Task<TransactionRequest> CreateTransactionRequest(CommandTransactionType aftOutType, long amount, uint handPayType)
        {
            var selectedDenom = _propertiesManager.GetValue(GamingConstants.SelectedDenom, 0L);

            return new TransactionRequest
            {
                RequestTimeout = new LockupRequestTimeout
                {
                    LockupKey = HhrConstants.CreditOutCmdTransactionErrorKey,
                    LockupString = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CreditOutCmdFailedMsg),
                    LockupHelpText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CreditOutCmdFailedHelpMsg)
                },
                TransactionId = (uint)_transactionIdProvider.GetNextTransactionId(),
                TransactionType = aftOutType,
                PlayerId = await _playerSessionService.GetCurrentPlayerId(),
                Credit = 0,
                Debit = (uint)amount.MillicentsToCents(),
                LastGamePlayTime = _propertiesManager.GetValue(HHRPropertyNames.LastGamePlayTime, 0u),
                GameMapId = await _gameDataService.GetDefaultGameMapIdAsync(),
                Flags = 0,
                Denomination = (uint)selectedDenom.MillicentsToCents(),
                HandpayType = handPayType,
                TimeoutInMilliseconds = HhrConstants.MsgTransactionTimeoutMs,
                RetryCount = HhrConstants.RetryCount,
                CashBalance = (uint)(_bank.QueryBalance(AccountType.Cashable) + _bank.QueryBalance(AccountType.Promo)).MillicentsToCents(),
                NonCashBalance = (uint)_bank.QueryBalance(AccountType.NonCash).MillicentsToCents()
            };
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<HandpayKeyedOffEvent>(this, Handle);
            _eventBus.Subscribe<VoucherIssuedEvent>(this, Handle);
            _eventBus.Subscribe<WatTransferCommittedEvent>(this, Handle);
        }

        private async void Handle(HandpayKeyedOffEvent evt)
        {
            // right now handling only the canceled credits as per section 9 of the messaging pdf.

            Logger.Debug($"Handling HandpayKeyedOffEvent for {evt.Transaction.TransactionAmount}");
            if (evt.Transaction.HandpayType != Accounting.Contracts.Handpay.HandpayType.CancelCredit)
            {
                return;
            }

            await TrySend(
                CommandTransactionType.CreditsCancelled,
                evt.Transaction.CashableAmount,
                HHRHandpayType.HandpayTypeCancelledCredits);
        }

        //Todo: Need to add support for ForcedCashOut when GameWin happens
        private async void Handle(VoucherIssuedEvent evt)
        {
            // NOTE: This doesn't occur for game wins, only for cashing out.
            Logger.Debug($"Handling VoucherIssuedEvent for {evt.Transaction.TransactionAmount}");
            if (evt.Transaction.Amount == 0)
            {
                return;
            }

            CommandTransactionType transType;

            switch (evt.Transaction.TypeOfAccount)
            {
                case AccountType.Cashable:
                    transType = CommandTransactionType.TicketPrinted;
                    break;
                case AccountType.NonCash:
                    transType = CommandTransactionType.TicketPrintedNonCashable;
                    break;
                case AccountType.Promo:
                    transType = CommandTransactionType.TicketPrintedPromo;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await TrySend(
                transType,
                evt.Transaction.Amount,
                HHRHandpayType.HandpayTypeNone);
        }

        private async void Handle(WatTransferCommittedEvent evt)
        {
            Logger.Debug($"Handling WatTransferCommittedEvent for {evt.Transaction.TransactionAmount}");

            if (evt.Transaction.TransactionAmount == 0) return;

            if (evt.Transaction.TransferredNonCashAmount > 0)
            {
                await TrySend(
                    CommandTransactionType.AftOutNonCashable,
                    evt.Transaction.TransferredNonCashAmount,
                    HHRHandpayType.HandpayTypeNone);
            }

            if (evt.Transaction.TransferredPromoAmount > 0)
            {
                await TrySend(
                    CommandTransactionType.AftOutPromo,
                    evt.Transaction.TransferredPromoAmount,
                    HHRHandpayType.HandpayTypeNone);
            }

            if (evt.Transaction.TransferredCashableAmount > 0)
            {
                await TrySend(
                    CommandTransactionType.AftOutCashable,
                    evt.Transaction.TransferredCashableAmount,
                    HHRHandpayType.HandpayTypeNone);
            }
        }

        private async Task TrySend(CommandTransactionType type, long amount, HHRHandpayType handPayType)
        {
            if (amount == 0)
                return;

            try
            {
                await _centralManager.Send<TransactionRequest, CloseTranResponse>(
                    await CreateTransactionRequest(
                        type,
                        amount,
                        (uint) handPayType));
            }
            catch (UnexpectedResponseException respEx)
            {
                Logger.Error($"Failed to send credit out message for (Type={type}, Amount={amount})", respEx);

                HandleResponseError(respEx);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error($"No player ID available for credit out message for (Type={type}, Amount={amount})", ex);

                HandleResponseError(null);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to send credit out message for (Type={type}, Amount={amount})", ex);

                HandleResponseError(null);
            }
        }

        private void HandleResponseError(UnexpectedResponseException respEx)
        {
            if (respEx?.Response is CloseTranErrorResponse transErrorResponse)
            {
                Logger.Error($"Error Response : ({transErrorResponse.ErrorCode} - {transErrorResponse.ErrorText})");
            }
        }
    }
}