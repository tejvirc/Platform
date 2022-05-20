namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Accounting.Contracts;
    using Accounting.Contracts.Vouchers;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Base;
    using Common;
    using Contracts;
    using Contracts.Client;
    using Contracts.Events;
    using Handlers;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Stateless;
    using Storage;
    using Storage.Models;
    using Ticketing;
    using Timer = System.Timers.Timer;

    /// <summary>
    ///     An implementation of <see cref="ISasVoucherInProvider"/>
    /// </summary>
    public class SasVoucherInProvider : ISasVoucherInProvider, IDisposable
    {
        private const int ResendingException68Interval = 15000;
        private const int WaitForLongPoll70Timeout = 10000;
        private const int RedemptionTimeout = 30000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IEventBus _bus;
        private readonly SasExceptionTimer _sasExceptionTimer;
        private readonly StateMachine<SasVoucherInState, SasVoucherInTriggers> _validationStateMachine;
        private readonly object _guard = new object();
        private readonly Timer _redemptionPendingTimer = new Timer { AutoReset = false };
        private readonly TicketInInfo _tempTicketData = new TicketInInfo();
        private readonly ITransactionHistory _transactionHistory;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ITicketingCoordinator _ticketingCoordinator;
        private readonly IBank _bank;
        private TaskCompletionSource<TicketInInfo> _ticketInValidationTask;
        private bool _disposed;

        private StateMachine<SasVoucherInState, SasVoucherInTriggers>.TriggerWithParameters<RedeemTicketData>
            _acceptedRestrictedTrigger;

        private StateMachine<SasVoucherInState, SasVoucherInTriggers>.TriggerWithParameters<RedemptionStatusCode>
            _voucherCommittedTrigger;

        private StateMachine<SasVoucherInState, SasVoucherInTriggers>.TriggerWithParameters<RedemptionStatusCode>
            _rejectWithStatusTrigger;

        private TicketInInfo _ticketInInfo = new TicketInInfo();

        /// <summary>
        ///     Creates a SasVoucherInProvider Instance
        /// </summary>
        public SasVoucherInProvider(
            ISasExceptionHandler exceptionHandler,
            ITransactionHistory transactionHistory,
            IPropertiesManager propertiesManager,
            IEventBus bus,
            ITicketingCoordinator ticketingCoordinator,
            IBank bank)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _ticketingCoordinator =
                ticketingCoordinator ?? throw new ArgumentNullException(nameof(ticketingCoordinator));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _sasExceptionTimer = new SasExceptionTimer(
                _exceptionHandler,
                () => GeneralExceptionCode.TicketTransferComplete,
                () =>
                {
                    var transaction = _transactionHistory.RecallTransactions<VoucherInTransaction>()
                        .FirstOrDefault(t => t.TransactionId == _ticketInInfo.TransactionId);
                    return CurrentState == SasVoucherInState.AcknowledgementPending &&
                           (!transaction?.CommitAcknowledged ?? false);
                },
                ResendingException68Interval);

            _redemptionPendingTimer.Elapsed += HandleWaitForLongPoll70Timeout;

            _validationStateMachine = BuildStateMachine(Recover());
            _bus.Subscribe<HostOfflineEvent>(this, Handle);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public SasVoucherInState CurrentState => _validationStateMachine.State;

        /// <inheritdoc />
        public TicketInInfo CurrentTicketInfo => (TicketInInfo)_ticketInInfo.Clone();

        /// <inheritdoc />
        public RedemptionStatusCode CurrentRedemptionStatus => _ticketInInfo.RedemptionStatusCode;

        /// <inheritdoc />
        public bool RedemptionEnabled => _propertiesManager.GetValue(PropertyKey.VoucherIn, false);

        /// <inheritdoc />
        public async Task<TicketInInfo> ValidationTicket(VoucherInTransaction transaction)
        {
            _ticketInValidationTask?.TrySetCanceled(); // Attempt to cancel any current running task

            if (!_validationStateMachine.CanFire(SasVoucherInTriggers.TicketValidationRequest))
            {
                UpdateTransaction(transaction.TransactionId);
                return new TicketInInfo
                {
                    Barcode = transaction.Barcode,
                    RedemptionStatusCode = RedemptionStatusCode.TicketRejectedByHost,
                    Amount = 0UL
                };
            }

            _ticketInValidationTask = new TaskCompletionSource<TicketInInfo>();

            //We need two "copies" of the ticket data, because we could get a request for status when another ticket is inserted,
            //but before we get the 70 for that ticket.  This will result in the last ticket status being sent.
            _tempTicketData.Amount = 0;
            _tempTicketData.Barcode = transaction.Barcode;
            _tempTicketData.TransactionId = transaction.TransactionId;
            _tempTicketData.TransferCode = TicketTransferCode.UnableToValidate;
            _tempTicketData.RedemptionStatusCode = RedemptionStatusCode.TicketRejectedByHost;

            await _validationStateMachine.FireAsync(SasVoucherInTriggers.TicketValidationRequest);
            return await WaitForValidationInformation();
        }

        /// <inheritdoc />
        public void AcceptTicket(RedeemTicketData data, RedemptionStatusCode statusCode)
        {
            if (data.TransferCode == TicketTransferCode.ValidRestrictedPromotionalTicket)
            {
                //This method is what is called when the host sends us LP71
                if (!_validationStateMachine.CanFire(_acceptedRestrictedTrigger.Trigger))
                {
                    return;
                }

                _ticketInInfo.Amount = data.TransferAmount;
                _ticketInInfo.TransferCode = data.TransferCode;
                _ticketInInfo.RedemptionStatusCode = statusCode;

                _validationStateMachine.FireAsync(_acceptedRestrictedTrigger, data).FireAndForget();
            }
            else
            {
                //This method is what is called when the host sends us LP71
                if (!_validationStateMachine.CanFire(SasVoucherInTriggers.AcceptValidationData))
                {
                    return;
                }

                _ticketInInfo.Amount = data.TransferAmount;
                _ticketInInfo.TransferCode = data.TransferCode;
                _ticketInInfo.RedemptionStatusCode = statusCode;

                _validationStateMachine.FireAsync(SasVoucherInTriggers.AcceptValidationData).FireAndForget();
            }
        }

        /// <inheritdoc />
        public void DenyTicket(RedemptionStatusCode statusCode, TicketTransferCode? transferCode = null)
        {
            if (!_validationStateMachine.CanFire(_rejectWithStatusTrigger.Trigger))
            {
                return;
            }

            if (transferCode != null)
            {
                _ticketInInfo.TransferCode = transferCode.Value;
            }

            _validationStateMachine.Fire(_rejectWithStatusTrigger, statusCode);
        }

        /// <inheritdoc />
        public void DenyTicket()
        {
            if (!_validationStateMachine.CanFire(SasVoucherInTriggers.RejectTicket))
            {
                return;
            }

            _validationStateMachine.Fire(SasVoucherInTriggers.RejectTicket);
        }

        /// <inheritdoc />
        public SendTicketValidationDataResponse RequestValidationData()
        {
            //This method is what is called when the host sends us LP70.  We start a new redemption cycle.
            if (!_validationStateMachine.CanFire(SasVoucherInTriggers.RequestValidationData))
            {
                return null;
            }

            //Only save to the cache if we are starting a new redemption cycle
            _ticketInInfo.Amount = 0;
            _ticketInInfo.Barcode = _tempTicketData.Barcode;
            _ticketInInfo.TransactionId = _tempTicketData.TransactionId;
            _ticketInInfo.RedemptionStatusCode = RedemptionStatusCode.WaitingForLongPoll71;

            _validationStateMachine.Fire(SasVoucherInTriggers.RequestValidationData);

            return new SendTicketValidationDataResponse
            {
                ParsingCode = (byte)ParsingCode.Bcd, Barcode = _ticketInInfo.Barcode
            };
        }

        /// <inheritdoc />
        public RedeemTicketResponse RequestValidationStatus()
        {
            //This method is what is called when the host sends us LP71
            //Handle the case where we do not have a previous ticket.
            if (string.IsNullOrEmpty(_ticketInInfo.Barcode))
            {
                //The parser will omit transfer amount, parsing code, and validation data
                _ticketInInfo.RedemptionStatusCode = RedemptionStatusCode.NoValidationInfoAvailable;
            }

            return new RedeemTicketResponse
            {
                MachineStatus = _ticketInInfo.RedemptionStatusCode,
                TransferAmount = _ticketInInfo.Amount,
                ParsingCode = ParsingCode.Bcd,
                Barcode = _ticketInInfo.Barcode
            };
        }

        /// <inheritdoc />
        public void OnTicketInCompleted(AccountType accountType)
        {
            if (!_validationStateMachine.CanFire(_voucherCommittedTrigger.Trigger))
            {
                return;
            }

            switch (accountType)
            {
                case AccountType.Cashable:
                    _validationStateMachine.Fire(_voucherCommittedTrigger, RedemptionStatusCode.CashableTicketRedeemed);
                    break;
                case AccountType.NonCash:
                    _validationStateMachine.Fire(
                        _voucherCommittedTrigger,
                        RedemptionStatusCode.RestrictedPromotionalTicketRedeemed);
                    break;
                default:
                    _validationStateMachine.Fire(
                        _voucherCommittedTrigger,
                        RedemptionStatusCode.NonRestrictedPromotionalTicketRedeemed);
                    break;
            }
        }

        /// <inheritdoc />
        public void OnTicketInFailed(string barcode, RedemptionStatusCode statusCode, long transactionId)
        {
            if (CurrentState != SasVoucherInState.RequestPending)
            {
                UpdateTransaction(transactionId);
            }

            if (!_validationStateMachine.CanFire(_voucherCommittedTrigger.Trigger))
            {
                return;
            }

            _validationStateMachine.Fire(_voucherCommittedTrigger, statusCode);
        }

        /// <inheritdoc />
        public void RedemptionStatusAcknowledged()
        {
            if (!_validationStateMachine.CanFire(SasVoucherInTriggers.StatusAck))
            {
                return;
            }

            _sasExceptionTimer.StopTimer();
            _exceptionHandler.RemoveException(new GenericExceptionBuilder(GeneralExceptionCode.TicketTransferComplete));

            Logger.Debug($"SAS Host has acknowledged the {CurrentState}");
            AcknowledgeTicketTransfer(_ticketInInfo);

            _validationStateMachine.Fire(SasVoucherInTriggers.StatusAck);
        }

        /// <inheritdoc />
        public void SetRedemptionStatusCode(RedemptionStatusCode code)
        {
            lock (_guard)
            {
                _ticketInInfo.RedemptionStatusCode = code;
            }
        }

        /// <inheritdoc />
        public void OnSystemDisabled()
        {
            lock (_guard)
            {
                if (CurrentState == SasVoucherInState.ValidationRequestPending ||
                    CurrentState == SasVoucherInState.ValidationDataPending)
                {
                    DenyTicket(RedemptionStatusCode.GamingMachineUnableToAcceptTransfer);
                }
            }
        }

        /// <summary>
        ///     Handles the Dispose
        /// </summary>
        /// <param name="disposing">Whether or not to do disposal</param>
        protected virtual void Dispose(bool disposing)
        {
            lock (_guard)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    StopTimers();
                    _redemptionPendingTimer.Elapsed -= HandleWaitForLongPoll70Timeout;
                    _redemptionPendingTimer.Dispose();
                    _sasExceptionTimer.Dispose();
                    _bus.UnsubscribeAll(this);
                }

                _disposed = true;
            }
        }
        
        private void UpdateTransaction(long transactionId)
        {
            Task.Run(
                () =>
                {
                    var transaction = _transactionHistory.RecallTransactions<VoucherInTransaction>()
                        .FirstOrDefault(t => t.TransactionId == transactionId);

                    if (transaction != null && !transaction.CommitAcknowledged)
                    {
                        transaction.CommitAcknowledged = true;
                        _transactionHistory.UpdateTransaction(transaction);
                    }
                });
        }

        private void AcknowledgeTicketTransfer(TicketInInfo data)
        {
            Task.Run(
                async () =>
                {
                    var transaction = _transactionHistory.RecallTransactions<VoucherInTransaction>()
                        .FirstOrDefault(t => t.TransactionId == data.TransactionId);
                    var storageData = _ticketingCoordinator.GetData();
                    storageData.VoucherInState = CurrentState;
                    if (transaction != null && !transaction.CommitAcknowledged)
                    {
                        transaction.CommitAcknowledged = true;
                        _transactionHistory.UpdateTransaction(transaction);
                    }

                    await _ticketingCoordinator.Save(storageData);
                });
        }

        private SasVoucherInState Recover()
        {
            var storageData = _ticketingCoordinator.GetData();
            _ticketInInfo = StorageHelpers.Deserialize(storageData.TicketInfoField, () => new TicketInInfo());
            var lastState = storageData.VoucherInState;
            var transaction = _transactionHistory.RecallTransactions<VoucherInTransaction>()
                .FirstOrDefault(x => x.TransactionId == _ticketInInfo.TransactionId);

            Logger.Debug($"Starting recovery handling for ticketing with a last known state of {lastState}");
            if (lastState == SasVoucherInState.Idle || (transaction?.CommitAcknowledged ?? false))
            {
                return SasVoucherInState.Idle;
            }

            if (lastState == SasVoucherInState.AcknowledgementPending && !(transaction?.CommitAcknowledged ?? false))
            {
                _sasExceptionTimer.StartTimer();
                return SasVoucherInState.AcknowledgementPending;
            }

            switch (transaction?.State ?? VoucherState.Rejected)
            {
                case VoucherState.Redeemed:
                    // ReSharper disable once PossibleNullReferenceException this can't be null.  Null goes to rejected
                    _ticketInInfo.RedemptionStatusCode = transaction.TypeOfAccount.ToRedemptionStatusCode();
                    _sasExceptionTimer.StartTimer();
                    return SasVoucherInState.AcknowledgementPending;
                case VoucherState.Rejected:
                    if (lastState != SasVoucherInState.RequestPending &&
                        lastState != SasVoucherInState.ValidationDataPending &&
                        lastState != SasVoucherInState.AcknowledgementPending)
                    {
                        Logger.Debug($"Recovering with the last state {lastState} where SAS didn't call LP71 and no data needs to be recovered");
                        return SasVoucherInState.Idle;
                    }

                    if (_ticketInInfo.RedemptionStatusCode < RedemptionStatusCode.TicketRejectedByHost)
                    {
                        _ticketInInfo.RedemptionStatusCode =
                            ((VoucherInExceptionCode)(transaction?.Exception ??
                                                      (int)VoucherInExceptionCode.InvalidTicket))
                            .ToRedemptionStatusCode();
                        Logger.Debug(
                            $"Recovering and setting the ticket status code to {_ticketInInfo.RedemptionStatusCode}");
                    }

                    if (lastState == SasVoucherInState.ValidationDataPending)
                    {
                        return SasVoucherInState.Idle;
                    }

                    _sasExceptionTimer.StartTimer();
                    return SasVoucherInState.AcknowledgementPending;
                default:
                    return lastState;
            }
        }

        private async Task<TicketInInfo> WaitForValidationInformation()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                if (_ticketInValidationTask.Task == await Task.WhenAny(
                    _ticketInValidationTask.Task,
                    Task.Delay(TimeSpan.FromMilliseconds(RedemptionTimeout), cancellation.Token))
                ) // Use 71 timeout as it is from ticket inset time and 3 times as long as 70
                {
                    cancellation.Cancel();
                    return await _ticketInValidationTask.Task;
                }

                _ticketInValidationTask.TrySetCanceled();
                if (CurrentState == SasVoucherInState.ValidationDataPending)
                {
                    _ticketInInfo.RedemptionStatusCode = RedemptionStatusCode.TicketRejectedDueToTimeout;
                    _ticketInInfo.TransferCode = TicketTransferCode.RequestForCurrentTicketStatus;
                }

                await _validationStateMachine.FireAsync(SasVoucherInTriggers.ValidationTimedOut);
                return CreateDeniedTicket(
                    CurrentState == SasVoucherInState.ValidationRequestPending ? _tempTicketData : _ticketInInfo);
            }
        }

        private StateMachine<SasVoucherInState, SasVoucherInTriggers> BuildStateMachine(
            SasVoucherInState voucherInState)
        {
            var stateMachine =
                new StateMachine<SasVoucherInState, SasVoucherInTriggers>(voucherInState); // Load with correct state
            _acceptedRestrictedTrigger =
                stateMachine.SetTriggerParameters<RedeemTicketData>(
                    SasVoucherInTriggers.AcceptRestrictedValidationData);
            _voucherCommittedTrigger =
                stateMachine.SetTriggerParameters<RedemptionStatusCode>(SasVoucherInTriggers.VoucherCommitted);
            _rejectWithStatusTrigger =
                stateMachine.SetTriggerParameters<RedemptionStatusCode>(SasVoucherInTriggers.RejectWithStatus);

            stateMachine.Configure(SasVoucherInState.Idle)
                .Permit(SasVoucherInTriggers.TicketValidationRequest, SasVoucherInState.ValidationRequestPending)
                .OnEntryFrom(_voucherCommittedTrigger, status => _ticketInInfo.RedemptionStatusCode = status)
                .OnEntryFrom(
                    SasVoucherInTriggers.RejectTicket,
                    () => _ticketInValidationTask?.TrySetResult(CreateDeniedTicket(_tempTicketData)))
                .OnEntryFrom(
                    _rejectWithStatusTrigger,
                    statusCode =>
                    {
                        _ticketInInfo.RedemptionStatusCode = statusCode;
                        _ticketInValidationTask?.TrySetResult(CreateDeniedTicket(_ticketInInfo));
                    })
                .OnEntry(OnIdle);

            stateMachine.Configure(SasVoucherInState.ValidationRequestPending) //Ticket has been inserted get ready to post 67.
                .Permit(SasVoucherInTriggers.RequestValidationData, SasVoucherInState.ValidationDataPending)
                .Permit(SasVoucherInTriggers.ValidationTimedOut, SasVoucherInState.Idle)
                .Permit(SasVoucherInTriggers.RejectTicket, SasVoucherInState.Idle)
                .PermitReentry(SasVoucherInTriggers.StatusAck)
                .OnEntryFrom(
                    SasVoucherInTriggers.TicketValidationRequest,
                    () =>
                    {
                        _exceptionHandler.ReportException(
                            new GenericExceptionBuilder(GeneralExceptionCode.TicketHasBeenInserted));
                        StartTimers();
                    });

            stateMachine.Configure(SasVoucherInState.ValidationRequestPendingWithAcknowledgementPending) //Ticket has been inserted get ready to post 67.
                .Permit(SasVoucherInTriggers.RequestValidationData, SasVoucherInState.ValidationDataPending)
                .Permit(SasVoucherInTriggers.ValidationTimedOut, SasVoucherInState.AcknowledgementPending)
                .Permit(SasVoucherInTriggers.RejectTicket, SasVoucherInState.AcknowledgementPending)
                .PermitReentry(SasVoucherInTriggers.StatusAck)
                .OnEntryFrom(
                    SasVoucherInTriggers.TicketValidationRequest,
                    () =>
                    {
                        _exceptionHandler.ReportException(
                            new GenericExceptionBuilder(GeneralExceptionCode.TicketHasBeenInserted));
                        StartTimers();
                    });

            stateMachine.Configure(SasVoucherInState.ValidationDataPending) //Got back the 70.
                .Permit(SasVoucherInTriggers.ValidationTimedOut, SasVoucherInState.Idle)
                .Permit(SasVoucherInTriggers.AcceptValidationData, SasVoucherInState.RequestPending)
                .Permit(SasVoucherInTriggers.AcceptRestrictedValidationData, SasVoucherInState.RequestPending)
                .Permit(SasVoucherInTriggers.RejectWithStatus, SasVoucherInState.Idle)
                .Permit(SasVoucherInTriggers.VoucherCommitted, SasVoucherInState.Idle)
                .OnEntry(OnValidationDataPending);

            stateMachine
                .Configure(SasVoucherInState.RequestPending) //We got 71, and we are pending, respond with status 40
                .PermitReentry(SasVoucherInTriggers.RequestValidationData)
                .Permit(SasVoucherInTriggers.VoucherCommitted, SasVoucherInState.AcknowledgementPending)
                .OnEntryFromAsync(_acceptedRestrictedTrigger, OnRequestPending)
                .OnEntryFromAsync(SasVoucherInTriggers.AcceptValidationData, OnRequestPending);

            stateMachine.Configure(SasVoucherInState.AcknowledgementPending)
                .Permit(SasVoucherInTriggers.StatusAck, SasVoucherInState.Idle)
                .Permit(SasVoucherInTriggers.TicketValidationRequest, SasVoucherInState.ValidationRequestPendingWithAcknowledgementPending)
                .OnEntryFrom(_voucherCommittedTrigger, status => OnAcknowledgePending(status).FireAndForget())
                .OnEntryFrom(
                    SasVoucherInTriggers.RejectTicket,
                    () =>
                    {
                        _ticketInValidationTask?.TrySetResult(CreateDeniedTicket(_tempTicketData));
                        _sasExceptionTimer.StartTimer();
                    });

            stateMachine.OnTransitioned(
                transition => Logger.Debug(
                    $"Transitioned From: {transition.Source} To: {transition.Destination} Trigger: {transition.Trigger}"));
            stateMachine.OnUnhandledTrigger(
                (state, trigger) => Logger.Error($"Invalid Transition. State: {state} Trigger: {trigger}"));

            return stateMachine;
        }

        private void OnIdle()
        {
            StopTimers();
            var ticketStorageData = _ticketingCoordinator.GetData();
            ticketStorageData.TicketInfoField = StorageHelpers.Serialize(_ticketInInfo);
            ticketStorageData.VoucherInState = CurrentState;
            _ticketingCoordinator.Save(ticketStorageData).FireAndForget();
            _sasExceptionTimer.StopTimer();
        }

        private void OnValidationDataPending()
        {
            StopWaitForLongPoll70Timer();
            _sasExceptionTimer.StopTimer(); // No need to keep posting 68 the status is now lost
            var ticketStorageData = _ticketingCoordinator.GetData();
            ticketStorageData.TicketInfoField = StorageHelpers.Serialize(_ticketInInfo);
            ticketStorageData.VoucherInState = CurrentState;
            _ticketingCoordinator.Save(ticketStorageData).FireAndForget();
        }

        private async Task OnRequestPending()
        {
            var ticketStorageData = _ticketingCoordinator.GetData();
            ticketStorageData.TicketInfoField = StorageHelpers.Serialize(_ticketInInfo);
            ticketStorageData.VoucherInState = CurrentState;
            await _ticketingCoordinator.Save(ticketStorageData);
            _ticketInValidationTask?.TrySetResult(_ticketInInfo);
        }

        private async Task OnAcknowledgePending(RedemptionStatusCode status)
        {
            _ticketInInfo.RedemptionStatusCode = status;
            _sasExceptionTimer.StartTimer();
            var ticketStorageData = _ticketingCoordinator.GetData();
            ticketStorageData.TicketInfoField = StorageHelpers.Serialize(_ticketInInfo);
            ticketStorageData.VoucherInState = CurrentState;
            await _ticketingCoordinator.Save(ticketStorageData);

        }

        private async Task OnRequestPending(RedeemTicketData data)
        {
            var ticketStorageData = _ticketingCoordinator.GetData();
            ticketStorageData.PoolId = data.PoolId;
            if (data.RestrictedExpiration > 0)
            {
                ticketStorageData.SetRestrictedExpirationWithPriority(
                    (int)data.RestrictedExpiration,
                    ticketStorageData.GetHighestPriorityExpiration(),
                    _bank.QueryBalance(AccountType.NonCash));
            }

            ticketStorageData.TicketInfoField = StorageHelpers.Serialize(_ticketInInfo);
            ticketStorageData.VoucherInState = CurrentState;
            await _ticketingCoordinator.Save(ticketStorageData);
            _ticketInValidationTask?.TrySetResult(_ticketInInfo);
        }

        private TicketInInfo CreateDeniedTicket(TicketInInfo ticket)
        {
            return new TicketInInfo
            {
                Amount = 0L,
                RedemptionStatusCode = ticket.RedemptionStatusCode,
                Barcode = ticket.Barcode,
                TransferCode = ticket.TransferCode
            };
        }

        private void StartTimers()
        {
            lock (_guard)
            {
                if (_disposed)
                {
                    return;
                }

                StopTimers();
                _redemptionPendingTimer.Interval = WaitForLongPoll70Timeout;
                _redemptionPendingTimer.Start();
            }
        }

        private void StopTimers()
        {
            lock (_guard)
            {
                StopWaitForLongPoll70Timer();
            }
        }

        private void StopWaitForLongPoll70Timer()
        {
            lock (_guard)
            {
                if (_disposed)
                {
                    return;
                }

                _redemptionPendingTimer.Stop();
            }
        }

        private void HandleWaitForLongPoll70Timeout(object sender, ElapsedEventArgs e)
        {
            lock (_guard)
            {
                Logger.Info("[SAS] [REDEMPTION] Timed out when waiting for the long poll 70!");
                DenyTicket();
            }
        }

        private void Handle(HostOfflineEvent evt)
        {
            if (CurrentState == SasVoucherInState.ValidationDataPending)
            {
                DenyTicket(RedemptionStatusCode.TicketRejectedDueToCommunicationLinkDown);
            }
        }

        private enum SasVoucherInTriggers
        {
            TicketValidationRequest,
            RequestValidationData,
            RejectTicket,
            AcceptValidationData,
            AcceptRestrictedValidationData,
            VoucherCommitted,
            StatusAck,
            ValidationTimedOut,
            RejectWithStatus
        }
    }
}