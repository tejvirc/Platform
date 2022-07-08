namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Client.Messages;
    using Client.WorkFlow;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using Storage.Helpers;
    using log4net;
    using HandpayType = Accounting.Contracts.Handpay.HandpayType;
    using HHRHandpayType = Client.Messages.HandpayType;

    /// <summary>
    ///     Service responsible for monitoring Game Result events in platform and send appropriate commands to CentralServer
    ///     when credits are transferred out of the EGM.
    ///     Includes cash, tickets and AFT.
    /// </summary>
    public class GameWinService : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly ICentralManager _centralManager;
        private readonly IPlayerSessionService _playerSessionService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IBank _bank;
        private readonly IPrizeInformationEntityHelper _prizeInfoHelper;
        private readonly ITransactionIdProvider _transactionIdProvider;

        private bool _disposed;

        /// <summary>
        ///     Constructor to initialize GameWinService where it will subscribe for the Game Result event and send messages to
        ///     CentralServer.
        /// </summary>
        /// <param name="eventBus">EventBus to subscribe events from platform.</param>
        /// <param name="centralManager">ICentralManager implementation, to send messages to central server.</param>
        /// <param name="playerSessionService">The Player Session Service</param>
        /// <param name="propertiesManager">The Properties Manager Service</param>
        /// <param name="bank">The Bank service</param>
        /// <param name="prizeInfoHelper">The prize information entity helper</param>
        /// <param name="transactionIdProvider">Provides with next transaction Id</param>
        public GameWinService(
            IEventBus eventBus,
            ICentralManager centralManager,
            IPlayerSessionService playerSessionService,
            IPropertiesManager propertiesManager,
            IBank bank,
            IPrizeInformationEntityHelper prizeInfoHelper,
            ITransactionIdProvider transactionIdProvider)
        {
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));
            _centralManager = centralManager
                ?? throw new ArgumentNullException(nameof(centralManager));
            _playerSessionService = playerSessionService
                ?? throw new ArgumentNullException(nameof(playerSessionService));
            _propertiesManager = propertiesManager
                ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bank = bank
                ?? throw new ArgumentNullException(nameof(bank));
            _prizeInfoHelper = prizeInfoHelper
                ?? throw new ArgumentNullException(nameof(prizeInfoHelper));
            _transactionIdProvider = transactionIdProvider
                ?? throw new ArgumentNullException(nameof(transactionIdProvider));

            _eventBus.Subscribe<GameResultEvent>(this, Handle);
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

        private void Handle(GameResultEvent evt)
        {
            foreach (var transactionInfo in evt.Log.Transactions)
            {
                SendTransactionRequest(transactionInfo, (uint)evt.Denomination.MillicentsToCents());
            }
        }

        private async void SendTransactionRequest(TransactionInfo transactionInfo, uint denomination)
        {
            var prizeInfo = _prizeInfoHelper.PrizeInformation;
            uint gameId = prizeInfo?.GameMapId ?? 0;
            long amountInCents = transactionInfo.Amount.MillicentsToCents();

            if (transactionInfo.TransactionType == typeof(HandpayTransaction) &&
                transactionInfo.HandpayType == HandpayType.CancelCredit && amountInCents > 0)
            {
                await SendMessage(
                    denomination,
                    gameId,
                    CommandTransactionType.GameWinToHandpayNoReceipt,
                    amountInCents,
                    HHRHandpayType.HandpayTypeCancelledCredits);
            }
            else if (transactionInfo.TransactionType == typeof(WatTransaction) && amountInCents > 0)
            {
                await SendMessage(
                    denomination,
                    gameId,
                    CommandTransactionType.GameWinToAftHost,
                    amountInCents,
                    HHRHandpayType.HandpayTypeNone);
            }
            else if (transactionInfo.TransactionType == typeof(VoucherOutTransaction))
            {
                var totalProgressiveWin = prizeInfo.TotalProgressiveAmountWon;
                var totalNonProgressiveWin = prizeInfo.RaceSet1AmountWon
                                             + prizeInfo.RaceSet1ExtraWinnings
                                             + prizeInfo.RaceSet2AmountWonWithoutProgressives
                                             + prizeInfo.RaceSet2ExtraWinnings;

                Debug.Assert(totalProgressiveWin + totalNonProgressiveWin == amountInCents);

                if (totalProgressiveWin > 0)
                {
                    await SendMessage(
                        denomination,
                        gameId,
                        CommandTransactionType.GameWinToCashableOutTicket,
                        totalProgressiveWin,
                        HHRHandpayType.HandpayTypeProgressive);
                }

                if (totalNonProgressiveWin > 0)
                {
                    await SendMessage(
                        denomination,
                        gameId,
                        CommandTransactionType.GameWinToCashableOutTicket,
                        totalNonProgressiveWin,
                        HHRHandpayType.HandpayTypeNonProgressive);
                }
            }
        }

        private async Task SendMessage(
            uint denomination,
            uint gameId,
            CommandTransactionType type,
            long amountInCents,
            HHRHandpayType handpayType)
        {
            try
            {
                await _centralManager.Send<TransactionRequest, CloseTranResponse>(
                    new TransactionRequest
                    {
                        RequestTimeout =
                            new LockupRequestTimeout
                            {
                                LockupKey = HhrConstants.CreditOutCmdTransactionErrorKey,
                                LockupString =
                                    Localizer.For(CultureFor.Operator)
                                        .GetString(ResourceKeys.CreditOutCmdFailedMsg),
                                LockupHelpText = Localizer.For(CultureFor.Operator)
                                    .GetString(ResourceKeys.CreditOutCmdFailedHelpMsg)
                            },
                        TransactionId = (uint)_transactionIdProvider.GetNextTransactionId(),
                        TransactionType = type,
                        PlayerId = await _playerSessionService.GetCurrentPlayerId(),
                        Credit = (uint)amountInCents,
                        Debit = 0,
                        LastGamePlayTime = _propertiesManager.GetValue(HHRPropertyNames.LastGamePlayTime, 0u),
                        GameMapId = gameId,
                        Flags = 0,
                        Denomination = denomination,
                        HandpayType = (uint)handpayType,
                        TimeoutInMilliseconds = HhrConstants.MsgTransactionTimeoutMs,
                        RetryCount = HhrConstants.RetryCount,
                        CashBalance =
                            (uint)(_bank.QueryBalance(AccountType.Cashable) + _bank.QueryBalance(AccountType.Promo))
                            .MillicentsToCents(),
                        NonCashBalance = (uint)_bank.QueryBalance(AccountType.NonCash).MillicentsToCents()
                    });
            }
            catch (UnexpectedResponseException respEx)
            {
                Logger.Error($"Failed to send game win message for (Type={type}, Amount={amountInCents})", respEx);

                if (respEx.Response is CloseTranErrorResponse transErrorResponse)
                {
                    Logger.Error($"Error Response : ({transErrorResponse.ErrorCode} - {transErrorResponse.ErrorText})");
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error($"No player ID available for game win message for (Type={type}, Amount={amountInCents})", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to send game win message for (Type={type}, Amount={amountInCents})", ex);
            }
        }
    }
}