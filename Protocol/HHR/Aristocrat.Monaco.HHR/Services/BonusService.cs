namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Client.Messages;
    using Client.WorkFlow;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Payment;
    using Kernel;
    using Localization.Properties;
    using log4net;

    public class BonusService : IBonusPaymentDeterminationHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IBank _bank;
        private readonly ICentralManager _centralManager;

        private readonly IEventBus _eventBus;
        private readonly ITransactionIdProvider _transactionIdProvider;
        private readonly IPlayerSessionService _playerSessionService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameDataService _gameDataService;
        private readonly IPaymentDeterminationProvider _paymentDeterminationProvider;

        private bool _disposed;

        public BonusService(
            IEventBus eventBus,
            ICentralManager centralManager,
            IPlayerSessionService playerSessionService,
            IBank bank,
            ITransactionIdProvider transactionIdProvider,
            IPropertiesManager propertiesManager,
            IPaymentDeterminationProvider paymentDeterminationProvider,
            IGameDataService gameDataService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _centralManager = centralManager ?? throw new ArgumentNullException(nameof(centralManager));
            _playerSessionService = playerSessionService ?? throw new ArgumentNullException(nameof(playerSessionService));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactionIdProvider = transactionIdProvider ?? throw new ArgumentNullException(nameof(transactionIdProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));
            _paymentDeterminationProvider = paymentDeterminationProvider ?? throw new ArgumentNullException(nameof(paymentDeterminationProvider));

            // Register as Handler which will decided what to do with bonus awards.
            _paymentDeterminationProvider.BonusHandler = this;

            _eventBus.Subscribe<BonusAwardedEvent>(this, Handle);
        }

        public PayMethod GetBonusPayMethod(BonusTransaction transaction, long bonusAmountInMillicents)
        {
            // Since the other types don't hit the credit meter they can be skipped (hand pay can be keyed off to the credit meter, but that's handled elsewhere)
            if (transaction.PayMethod != PayMethod.Any && transaction.PayMethod != PayMethod.Credit)
            {
                return transaction.PayMethod;
            }

            var maxCreditLimit = _propertiesManager.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue);
            var winLimitCents = _propertiesManager.GetValue(
                AccountingConstants.LargeWinLimit,
                AccountingConstants.DefaultLargeWinLimit).MillicentsToCents();

            if (DoesBonusAmountRequireHandPay(
                bonusAmountInMillicents.MillicentsToCents(),
                winLimitCents))
            {
                transaction.PayMethod = PayMethod.Handpay;
            }
            else if (bonusAmountInMillicents + _bank.QueryBalance() > maxCreditLimit)
            {
                if (transaction.PayMethod != PayMethod.Any)
                {
                    throw new TransactionException(
                        $"Pay method not supported. Voucher required. Requested: {transaction.PayMethod}");
                }

                transaction.PayMethod = PayMethod.Voucher;
            }

            return transaction.PayMethod;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _paymentDeterminationProvider.UnregisterBonusHandler();

            }

            _disposed = true;
        }

        private static bool DoesBonusAmountRequireHandPay(
            decimal totalBonusCents,
            decimal winLimitCents)
        {
            return totalBonusCents > winLimitCents;
        }

        private void Handle(BonusAwardedEvent evt)
        {
            if (evt.Transaction.TotalAmount == 0) return;

            var trans = evt.Transaction;

            try
            {
                switch (trans.PayMethod)
                {
                    case PayMethod.Any:
                    case PayMethod.Credit:
                        // If credit limit not exceeded, the amount went to credit meter

                        if (trans.PaidNonCashAmount != 0)
                        {
                            TrySend(trans.PaidNonCashAmount, CommandTransactionType.BonusWinToCreditMeter, BonusCreditType.NonCash);
                        }

                        if (trans.PaidCashableAmount != 0)
                        {
                            TrySend(trans.PaidCashableAmount, CommandTransactionType.BonusWinToCreditMeter, BonusCreditType.Cash);
                        }

                        if (trans.PaidPromoAmount != 0)
                        {
                            TrySend(trans.PaidPromoAmount, CommandTransactionType.BonusWinToCreditMeter, BonusCreditType.Promo);
                        }

                        break;
                    case PayMethod.Voucher:
                        // If credit meter limit exceeded, the amount went to voucher
                        TrySend(trans.TotalAmount, CommandTransactionType.BonusWinToCashableOutTicket, BonusCreditType.Cash);

                        break;
                    case PayMethod.Handpay:
                        // If IRS limit exceeded, the amount went to hand pay
                        TrySend(trans.TotalAmount, CommandTransactionType.BonusWinToHandpayNoReceipt, BonusCreditType.Cash, HandpayType.HandpayTypeCancelledCredits);

                        break;
                    default:

                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (UnexpectedResponseException respEx)
            {
                Logger.Error("Failed to send bonus awarded  message", respEx);

                if (respEx.Response is CloseTranErrorResponse transErrorResponse)
                {
                    Logger.Error($"Error Response : ({transErrorResponse.ErrorCode} - {transErrorResponse.ErrorText})");
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("No player ID available for bonus award message", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send bonus award message", ex);
            }
        }

        private async Task<TransactionRequest> CreateTransactionRequest(
            long amountMillicents, CommandTransactionType transType,
            uint flags = 0, HandpayType handPayType = HandpayType.HandpayTypeNone)
        {
            return new TransactionRequest
            {
                PlayerId = await _playerSessionService.GetCurrentPlayerId(),
                TransactionId = (uint)_transactionIdProvider.GetNextTransactionId(),
                TransactionType = transType,
                Credit = (uint)amountMillicents.MillicentsToCents(),
                Flags = flags,
                CashBalance = (uint)_bank.QueryBalance(AccountType.Cashable).MillicentsToCents() +
                              (uint)_bank.QueryBalance(AccountType.Promo).MillicentsToCents(),
                NonCashBalance = (uint)_bank.QueryBalance(AccountType.NonCash).MillicentsToCents(),
                LastGamePlayTime = _propertiesManager.GetValue(HHRPropertyNames.LastGamePlayTime, 0u),
                GameMapId = await _gameDataService.GetDefaultGameMapIdAsync(),
                TimeoutInMilliseconds = HhrConstants.MsgTransactionTimeoutMs,
                RetryCount = HhrConstants.RetryCount,
                HandpayType = (uint)handPayType,
                RequestTimeout = new LockupRequestTimeout
                {
                    LockupKey = HhrConstants.BonusCmdTransactionErrorKey,
                    LockupString = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BonusCmdFailedMsg),
                    LockupHelpText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BonusCmdFailedHelpMsg)
                },
            };
        }

        private void TrySend(long amountMillicents, CommandTransactionType transType, BonusCreditType flags, HandpayType handpayType = HandpayType.HandpayTypeNone)
        {
            try
            {
                // Because SonarQube will freak out if I make this function async void, we have to explicitly wait here.
                _centralManager.Send<TransactionRequest, CloseTranResponse>(
                    CreateTransactionRequest(
                        amountMillicents, transType, (uint)flags, handpayType).Result).Wait();
                Logger.Debug($"Bonus info sent : (TransType{transType}, Bonus credit type={flags})");
            }
            catch (UnexpectedResponseException respEx)
            {
                Logger.Error($"Failed to notify Bonus transaction - Unexpected response : (TransType{transType}, Bonus credit type={flags})", respEx);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error($"No player ID available for Bonus transaction : (TransType{transType}, Bonus credit type={flags})", ex);
            }
            catch (Exception ex)
            {
                Logger.Debug($"Failed to notify Bonus transaction : (TransType{transType}, Bonus credit type={flags})", ex);
            }
        }

        private enum BonusCreditType
        {
            Cash, // cash
            Promo, //non-cash, fake money - can be converted to cash. (SAS NonRestricted)
            NonCash //non-cash, fake money - cannot be converted to cash. (SAS Restricted)
        }
    }
}