namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using log4net;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary> definition of AftOffTransferProvider </summary>
    public sealed class AftOffTransferProvider :
        AftTransferProviderBase,
        IAftOffTransferProvider,
        IWatTransferOffProvider,
        ITransactionRequestor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly Guid TransactionOwnerId = new Guid("{1646C5E2-DAF3-4a0c-8F5D-E80896255A55}");

        private static readonly IReadOnlyList<PlayState> AvailableLockableStates = new List<PlayState>
        {
            PlayState.Idle, PlayState.PayGameResults, PlayState.PresentationIdle
        };

        private readonly object _lock = new object();

        /// <summary>A lock to synchronize access to the CurrentTransferState</summary>
        private readonly object _transferStateLock = new object();

        private readonly IWatOffProvider _watOffProvider;
        private readonly IHostCashOutProvider _hostCashOutProvider;
        private readonly IBank _bank;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IAftRegistrationProvider _aftRegistration;
        private readonly IEventBus _eventBus;
        private readonly IGamePlayState _gamePlay;

        /// <summary>Whether the transfer initiate request has been accepted or not.</summary>
        private bool _transferAccepted;

        /// <summary>Whether the transfer was client initiated or not (cash from host request).</summary>
        private bool _clientInitiated;

        /// <summary>Indicates whether transfers should be allowed</summary>
        private bool _allowTransfers;

        /// <summary>The reason transfers are disallowed</summary>
        private DisallowAftOffReason _reasonForDisallowingTransfers;

        /// <summary>Indicates if the the operator has keyed off the lockup.</summary>
        private bool _keyedOff;

        /// <summary>Indicates if a receipt is to be printed or not.</summary>
        private bool _printReceipt;

        private bool _serviceInitialized;

        /// <inheritdoc />
        public AftOffTransferProvider(
            ISasHost sasHost,
            IAftLockHandler aftLock,
            IWatOffProvider watOff,
            IHostCashOutProvider hostCashOutProvider,
            ISystemDisableManager systemDisableManager,
            ITransactionHistory transactionHistory,
            IAftRegistrationProvider registrationProvider,
            ITransactionCoordinator transactionCoordinator,
            IPropertiesManager propertiesManager,
            ITime timerService,
            IBank bank,
            IEventBus eventBus,
            IAutoPlayStatusProvider autoPlayStatusProvider,
            IGamePlayState gamePlay)
            : base(
                aftLock,
                sasHost,
                timerService,
                propertiesManager,
                transactionCoordinator,
                autoPlayStatusProvider)
        {
            _watOffProvider = watOff;
            _hostCashOutProvider = hostCashOutProvider;
            _systemDisableManager = systemDisableManager;
            _transactionHistory = transactionHistory;
            _bank = bank;
            _aftRegistration = registrationProvider;
            _eventBus = eventBus;
            _gamePlay = gamePlay;
        }

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes { get; } =
            new List<Type> { typeof(IAftOffTransferProvider), typeof(IWatTransferOffProvider) };

        /// <inheritdoc />
        public bool IsAftPending { get; private set; }

        /// <inheritdoc />
        public bool IsAftOffAvailable => AvailableLockableStates.Contains(_gamePlay.UncommittedState) &&
                                         (!IsAftDisabled || TransferOffInLockupAllowed);

        /// <summary>Gets or sets the current transfer state</summary>
        internal TransferOffState CurrentTransferState { get; set; }

        /// <inheritdoc />
        internal override bool HardCashOutMode => _hostCashOutProvider.CashOutMode == HostCashOutMode.Hard;

        private long AvailableTransferableAmount => _bank.QueryBalance();

        private bool MoneyToCashOff => AvailableTransferableAmount > 0;

        private bool AllowPartialTransfer =>
            CurrentTransfer?.TransferCode == AftTransferCode.TransferRequestPartialTransferAllowed;

        private bool ImmediateSystemDisable => _systemDisableManager.IsDisabled &&
                                               _systemDisableManager.DisableImmediately &&
                                               !TransferOffInLockupAllowed;

        private bool TransferOffInLockupAllowed =>
            // Allow transfer starts when we are validating the signatures so OASIS can use the UTJ Tax feature
            !_systemDisableManager.CurrentDisableKeys.Except(AftConstants.AllowedAftOffDisables).Any() ||
            _hostCashOutProvider.CanCashOut;

        /// <inheritdoc />
        public bool InitiateAftOff()
        {
            if (!_serviceInitialized)
            {
                Logger.Warn("Cannot initiate Aft Off before the initialization is done.");
                return false;
            }

            lock (_lock)
            {
                if (!_allowTransfers)
                {
                    Logger.Info($"Aft Off disallowed because {_reasonForDisallowingTransfers}");
                    return false;
                }
            }

            _transferAccepted = false;

            // If we are locked up for key off we can accept a transfer off for cashing out
            if (IsAftDisabled && !_hostCashOutProvider.CanCashOut)
            {
                // Accept a transfer of all zeros
                var transferAmounts = GetTransferAmountsDictionary(CurrentTransfer);
                if (transferAmounts.All(t => t.Value == 0))
                {
                    Logger.Info("Accepting a transfer of all zeros");
                }
                else
                {
                    Logger.Info($"Aft is disabled ({AftState}), Aft off initiate request denied.");
                    return false;
                }
            }

            if (_clientInitiated)
            {
                Logger.Debug("This Aft Off was client initiated and will be accepted.");
                _transferAccepted = true;
                return _transferAccepted;
            }

            lock (_lock)
            {
                // Aft Off was host initiated, check if it can be accepted
                RetrieveTransactionIdFromLock();

                if (TransactionId == Guid.Empty)
                {
                    if (AftLockHandler.LockStatus != AftGameLockStatus.GameLockPending)
                    {
                        // Request a transaction if SasLockHandler hasn't started to request it.
                        TransactionId = TransactionCoordinator.RequestTransaction(this, TransactionType.Write);
                    }
                }

                if (ImmediateSystemDisable)
                {
                    Logger.Debug("Immediate system disable, abandoning transaction if in progress");
                    if (TransactionId != Guid.Empty)
                    {
                        TransactionCoordinator.ReleaseTransaction(TransactionId);
                    }
                    else if (AftLockHandler.LockStatus != AftGameLockStatus.GameLockPending)
                    {
                        TransactionCoordinator.AbandonTransactions(RequestorGuid);
                    }

                    TransactionId = Guid.Empty;
                    return _transferAccepted;
                }

                if (TransactionId != Guid.Empty)
                {
                    _transferAccepted = true;
                }
                else
                {
                    IsAftPending = true;
                }

                Logger.Debug($"m_transferAccepted={_transferAccepted} IsAftPending={IsAftPending}");

                if (!_transferAccepted && !IsAftPending)
                {
                    ReleaseTransactionId();
                    ResetCashOutRequestState();
                }
            }

            if (!_transferAccepted && !IsAftPending)
            {
                TerminateLock();
            }

            Logger.Debug("End of InitiateAftOff()");
            return _transferAccepted;
        }

        /// <inheritdoc />
        public void CancelAftOff()
        {
            if (!_serviceInitialized)
            {
                Logger.Warn("Cannot cancel Aft Off before the initialization is done.");
                return;
            }

            Task.Run(
                () =>
                {
                    Logger.Debug("Canceling the Aft Off request...");

                    _watOffProvider.CancelTransfer(
                        CurrentTransfer.TransactionId,
                        (int)AftTransferStatusCode.TransferCanceledByHost);
                    lock (_lock)
                    {
                        ReleaseTransactionId();
                        _transferAccepted = false;
                        ResetCashOutRequestState();
                    }

                    TerminateLock();

                    lock (_lock)
                    {
                        if (_clientInitiated)
                        {
                            Logger.Debug("Set m_clientInitiated to false.");
                            _clientInitiated = false;
                        }

                        _allowTransfers = true;
                        _reasonForDisallowingTransfers = DisallowAftOffReason.None;
                    }

                    Logger.Debug("Ihe Aft Off request has been canceled!");
                });
        }

        /// <inheritdoc />
        public bool AftOffRequest(AftData data, bool partialAllowed)
        {
            if (!_serviceInitialized)
            {
                Logger.Warn("Client cannot request Aft Off before the initialization is done.");
                return false;
            }

            lock (_lock)
            {
                CurrentTransfer = data;
            }

            if (!InitiateAftOff() && !TransactionPending)
            {
                data.CashableAmount = 0;
                data.RestrictedAmount = 0;
                data.NonRestrictedAmount = 0;
                SasHost.AftTransferFailed(data, AftTransferStatusCode.GamingMachineUnableToPerformTransfer);
                return false;
            }

            lock (_lock)
            {
                if (ImmediateSystemDisable)
                {
                    Logger.Warn("Cannot process Aft Off while disabled!");
                    data.CashableAmount = 0;
                    data.RestrictedAmount = 0;
                    data.NonRestrictedAmount = 0;
                    SasHost.AftTransferFailed(data, AftTransferStatusCode.GamingMachineUnableToPerformTransfer);
                    return false;
                }

                CurrentTransfer = data;
                Logger.Debug($"partial transfer allowed {AllowPartialTransfer}");
                _hostCashOutProvider.CashOutAccepted();

                if (TransactionId != Guid.Empty)
                {
                    Logger.Debug("Continue performing Aft Off.");
                    return PerformAftOffRequest();
                }
            }

            return true;
        }

        /// <inheritdoc />
        public void AftOffRejected()
        {
        }

        /// <inheritdoc />
        public bool Recover(string transactionId)
        {
            var watTransaction = _transactionHistory.RecallTransactions<WatTransaction>()
                .FirstOrDefault(x => (x.Status != WatStatus.Complete) & (x.RequestId == transactionId));
            if (watTransaction == null)
            {
                // No need to recover anything if there is nothing to recover
                return false;
            }

            CurrentTransfer.TransactionId = transactionId;
            switch (watTransaction.Status)
            {
                case WatStatus.RequestReceived:
                case WatStatus.CancelReceived:
                case WatStatus.Initiated:
                case WatStatus.Authorized:
                    // Cancel the transaction we should not attempt to recover
                    _watOffProvider.CancelTransfer(watTransaction, (int)AftTransferStatusCode.UnexpectedError);
                    break;
                case WatStatus.Committed:
                    HandleTransferComplete(CurrentTransfer, watTransaction, Logger);
                    FinishAft(watTransaction);
                    break;
            }

            return true;
        }

        /// <inheritdoc cref="IAftOffTransferProvider" />
        public override bool WaitingForKeyOff => !_keyedOff && _hostCashOutProvider.LockedUp;

        /// <inheritdoc cref="IAftOffTransferProvider" />
        public override void OnKeyedOff()
        {
            lock (_lock)
            {
                if (_keyedOff || !_hostCashOutProvider.LockedUp)
                {
                    return;
                }

                Logger.DebugFormat("_keyedOff == {0}", _keyedOff);
                _eventBus.Publish(new HardCashKeyOffEvent());
                _keyedOff = true;
                _hostCashOutProvider.CashOutDenied();
            }
        }

        /// <inheritdoc />
        public void AcknowledgeTransfer(string transactionId)
        {
            var watTransaction = _transactionHistory.RecallTransactions<WatTransaction>()
                .FirstOrDefault(x => x.RequestId == transactionId);
            if (watTransaction != null)
            {
                _watOffProvider.AcknowledgeTransfer(watTransaction);
            }
        }

        /// <inheritdoc />
        public Guid RequestorGuid => TransactionOwnerId;

        /// <inheritdoc />
        public void NotifyTransactionReady(Guid requestId)
        {
            Task.Run(() => OnTransactionReady(requestId));
        }

        /// <inheritdoc />
        public bool CanTransfer => !IsAftDisabled &&
                                   // We can only accept the transfer if we can cashout or the host initiated the transfer
                                   (_hostCashOutProvider.CanCashOut || TransactionId != Guid.Empty);

        /// <inheritdoc />
        public async Task<bool> InitiateTransfer(WatTransaction transaction)
        {
            if (!_serviceInitialized)
            {
                Logger.Error("Platform cannot initiate a transfer before the initialization is done.");
                return await Task.FromResult(false);
            }

            if (transaction.Direction == WatDirection.HostInitiated)
            {
                Logger.Debug(
                    $"HostInitiated transfer off with cashable={transaction.CashableAmount}, restricted={transaction.NonCashAmount}, non-restricted={transaction.PromoAmount} partial={transaction.AllowReducedAmounts}");
                transaction.PayMethod = WatPayMethod.Credit;
                UpdateAuthorizedTransactionAmount(transaction);

                return true;
            }

            Logger.Debug("EGM initiating transfer off");
            var requestAccepted = false;

            if (!CashOutFromGamingMachineRequest && !_clientInitiated &&
                _hostCashOutProvider.CashOutMode != HostCashOutMode.None)
            {
                _clientInitiated = true;
                requestAccepted = await _hostCashOutProvider.HandleHostCashOut(transaction);
                if (requestAccepted)
                {
                    transaction.RequestId = CurrentTransfer.TransactionId;
                    UpdateAuthorizedHostCashoutAmount(transaction);
                }
                else
                {
                    ResetCashOutRequestState();
                }
            }

            Logger.Debug("Done with requesting transfer off!");
            return await Task.FromResult(requestAccepted);
        }

        /// <inheritdoc />
        public async Task CommitTransfer(WatTransaction transaction)
        {
            if (!_serviceInitialized)
            {
                Logger.Error("Platform cannot commit a transfer before the initialization is done.");
                return;
            }

            if (!string.IsNullOrEmpty(CurrentTransfer.TransactionId))
            {
                Logger.Debug("Completing transfer off...");

                HandleTransferComplete(CurrentTransfer, transaction, Logger);

                if (_printReceipt)
                {
                    // Update the Transfer Status so that it can be read before the ticket completes
                    FinishTransferStatus(transaction);

                    await PrintReceipt(SasAftReceiptCreator.CreateAftReceipt(CurrentTransfer, _aftRegistration));
                    _printReceipt = false;
                }

                await Task.Run(() => AftOffCommitted(transaction));
            }
            else
            {
                Logger.Error("TransferOffComplete(): invalid transaction!");
            }
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            Logger.Info("Initializing service...");

            base.Initialize();

            if (_systemDisableManager.IsDisabled && !_hostCashOutProvider.CanCashOut)
            {
                AftState |= AftDisableConditions.SystemDisabled;
            }

            if (ServiceManager.GetInstance().GetService<IOperatorMenuLauncher>().IsShowing)
            {
                AftState |= AftDisableConditions.OperatorMenuEntered;
            }

            TransactionCoordinator.AbandonTransactions(RequestorGuid);
            _allowTransfers = true;
            _reasonForDisallowingTransfers = DisallowAftOffReason.None;
            _serviceInitialized = true;

            Logger.Info("Service initialized!");
        }

        /// <inheritdoc />
        public override void OnSystemDisabled()
        {
            if (!_serviceInitialized)
            {
                Logger.Debug("System is disabled before the initialization is done. Not an issue!");
                return;
            }

            if (ImmediateSystemDisable)
            {
                AftState |= AftDisableConditions.SystemDisabled;
                Logger.DebugFormat("Canceling request, CurrentTransferState = {0}", CurrentTransferState.ToString());

                // Try to cancel the request if we were just disabled
                if (_watOffProvider.CancelTransfer(
                    CurrentTransfer.TransactionId,
                    (int)AftTransferStatusCode.GamingMachineUnableToPerformTransfer))
                {
                    Logger.Debug("Request was canceled.");
                    if (CurrentTransferState == TransferOffState.InitiatedWaiting)
                    {
                        lock (_transferStateLock)
                        {
                            CurrentTransferState = TransferOffState.Canceling;
                        }

                        _hostCashOutProvider.CashOutDenied();
                    }
                }
                else
                {
                    Logger.Debug("Request was not canceled, processing already started.");
                }
            }
            else
            {
                AftState &= ~AftDisableConditions.SystemDisabled;
            }
        }

        /// <inheritdoc />
        public override void OnStateChanged()
        {
            if (!_serviceInitialized)
            {
                Logger.Debug("state is changed before the initialization is done. Not an issue!");
                return;
            }

            AftDisabledByEvent(!IsAftDisabled);

            lock (_lock)
            {
                if (IsAftPending && TransactionId != Guid.Empty)
                {
                    PerformAftOffRequest();
                }
            }
        }

        /// <inheritdoc />
        protected override void AftDisabledByEvent(bool isEnabled)
        {
            Logger.DebugFormat("Setting Aft Out Enabled state to: {0}", isEnabled);
            if (!isEnabled &&
                !_hostCashOutProvider.CanCashOut &&
                (AftLockHandler.AftLockTransferConditions & AftTransferConditions.TransferFromGamingMachineOk) != AftTransferConditions.None)
            {
                TerminateLock();
            }
            SasHost.SetAftOutEnabled(isEnabled);
        }

        /// <inheritdoc />
        protected override void HandleLockAcquired()
        {
            lock (_lock)
            {
                if (IsAftPending)
                {
                    Logger.Debug("Handle the pending AftOff request.");
                    RetrieveTransactionIdFromLock();

                    PerformAftOffRequest();
                }
            }
        }

        /// <inheritdoc />
        internal override void ResetCashOutRequestState()
        {
            CashOutFromGamingMachineRequest = false;
            _keyedOff = false;
            _clientInitiated = false;
            CurrentTransfer = new AftData();

            lock (_lock)
            {
                _allowTransfers = true;
                _reasonForDisallowingTransfers = DisallowAftOffReason.None;
            }

            lock (_transferStateLock)
            {
                CurrentTransferState = TransferOffState.None;
            }
        }

        private void AftOffCommitted(WatTransaction transaction)
        {
            Logger.Debug(
                $"AftOffCommitted(): HardCashOutMode={HardCashOutMode}, MoneyToCashOff={MoneyToCashOff}, CashOutFromGamingMachineRequest={CashOutFromGamingMachineRequest}, m_clientInitiated={_clientInitiated}");

            if (!_serviceInitialized)
            {
                Logger.Error("Platform cannot commit Aft Off before the initialization is done.");
                return;
            }

            _clientInitiated = false;
            _transferAccepted = false;
            ReleaseTransactionId();
            TerminateLock();

            FinishAft(transaction);

            Logger.Debug("End of AftOffCommitted()!");
        }

        private void UpdateAuthorizedHostCashoutAmount(WatTransaction transaction)
        {
            if (AllowPartialTransfer)
            {
                var transferLimit = PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                    .TransferLimit.CentsToMillicents();
                transaction.AuthorizedCashableAmount = Math.Min(
                    Math.Min(transaction.CashableAmount, transferLimit),
                    ((long)CurrentTransfer.CashableAmount).CentsToMillicents());
                transferLimit -= transaction.AuthorizedCashableAmount;
                transaction.AuthorizedPromoAmount = Math.Min(
                    Math.Min(transaction.PromoAmount, transferLimit),
                    ((long)CurrentTransfer.NonRestrictedAmount).CentsToMillicents());
                transferLimit -= transaction.AuthorizedPromoAmount;
                transaction.AuthorizedNonCashAmount = Math.Min(
                    Math.Min(transaction.NonCashAmount, transferLimit),
                    ((long)CurrentTransfer.RestrictedAmount).CentsToMillicents());
            }
            else
            {
                transaction.AuthorizedCashableAmount = ((long)CurrentTransfer.CashableAmount).CentsToMillicents();
                transaction.AuthorizedPromoAmount = ((long)CurrentTransfer.NonRestrictedAmount).CentsToMillicents();
                transaction.AuthorizedNonCashAmount = ((long)CurrentTransfer.RestrictedAmount).CentsToMillicents();
            }
        }

        private void UpdateAuthorizedTransactionAmount(WatTransaction transaction)
        {
            if (transaction.AllowReducedAmounts)
            {
                var transferLimit = PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                    .TransferLimit.CentsToMillicents();
                transaction.AuthorizedCashableAmount = Math.Min(
                    transaction.CashableAmount,
                    Math.Min(_bank.QueryBalance(AccountType.Cashable), transferLimit));
                transferLimit -= transaction.AuthorizedCashableAmount;
                transaction.AuthorizedPromoAmount = Math.Min(
                    transaction.PromoAmount,
                    Math.Min(_bank.QueryBalance(AccountType.Promo), transferLimit));
                transferLimit -= transaction.AuthorizedPromoAmount;
                transaction.AuthorizedNonCashAmount = Math.Min(
                    transaction.NonCashAmount,
                    Math.Min(_bank.QueryBalance(AccountType.NonCash), transferLimit));
            }
            else
            {
                transaction.AuthorizedCashableAmount = transaction.CashableAmount;
                transaction.AuthorizedPromoAmount = transaction.PromoAmount;
                transaction.AuthorizedNonCashAmount = transaction.NonCashAmount;
            }
        }

        private void FinishTransferStatus(WatTransaction transaction)
        {
            if (transaction.HostException != 0 &&
                Enum.IsDefined(typeof(AftTransferStatusCode), transaction.HostException))
            {
                SasHost.AftTransferFailed(CurrentTransfer, (AftTransferStatusCode)transaction.HostException);
            }
            else if (transaction.EgmException != 0)
            {
                SasHost.AftTransferFailed(CurrentTransfer, AftTransferStatusCode.UnexpectedError);
            }
            else
            {
                SasHost.AftTransferComplete(CurrentTransfer);
            }
        }

        private void FinishAft(WatTransaction transaction)
        {
            FinishTransferStatus(transaction);

            ResetCashOutRequestState();
        }

        /// <summary>Called by the thread pool when the transaction is ready to retrieve.</summary>
        /// <param name="requestId">The request Id which the transaction is associated with.</param>
        private void OnTransactionReady(Guid requestId)
        {
            lock (_lock)
            {
                TransactionRequestId = requestId;
                TransactionId = TransactionCoordinator.RetrieveTransaction(TransactionRequestId);
                TransactionRequestId = Guid.Empty;

                PerformAftOffRequest();
                Logger.Debug("TransactionStartedEvent handled!");
            }
        }

        /// <summary>Performs the AftOff Request</summary>
        /// <returns>True if the request was or will be performed, false if it cannot be performed</returns>
        private bool PerformAftOffRequest()
        {
            Logger.Debug("Performing Aft Off request...");

            if (string.IsNullOrEmpty(CurrentTransfer.TransactionId) ||
                ImmediateSystemDisable)
            {
                ReleaseTransactionId();

                Logger.Debug("Cannot perform AftOff request.");
                return false;
            }

            if (_clientInitiated && !_transferAccepted)
            {
                Logger.Debug("Cannot perform AftOff request if the transfer is not initiated by host successfully.");
                return false;
            }

            IsAftPending = false;

            // If AftData Request Aft Receipt flag is set, set the flag to print a receipt.
            _printReceipt = (CurrentTransfer.TransferFlags & AftTransferFlags.TransactionReceiptRequested) ==
                            AftTransferFlags.TransactionReceiptRequested;

            SasHost.SetAftReceiptStatus(
                _printReceipt ? ReceiptStatus.ReceiptPending : ReceiptStatus.NoReceiptRequested);

            var transferAmounts = GetTransferAmountsDictionary(CurrentTransfer);

            // Check if we are going to request a host cash-out.
            if ((CurrentTransfer.TransferFlags & AftTransferFlags.CashOutFromGamingMachineRequest) ==
                AftTransferFlags.CashOutFromGamingMachineRequest)
            {
                CashOutFromGamingMachineRequest = true;

                // Listen for the Aft transaction to complete before performing the ticket out.
                TransactionPending = true;
            }

            var accepted = false;
            if (ImmediateSystemDisable)
            {
                Logger.Debug(
                    $"{MethodBase.GetCurrentMethod()!.Name}: Attempting to cancel transfer request while disabled...");
                _watOffProvider.CancelTransfer(
                    CurrentTransfer.TransactionId,
                    (int)AftTransferStatusCode.GamingMachineUnableToPerformTransfer);
            }
            else
            {
                lock (_transferStateLock)
                {
                    CurrentTransferState = TransferOffState.RequestedOff;
                }

                Logger.Debug($"allow partial transfers is {AllowPartialTransfer}");
                accepted = _watOffProvider.RequestTransfer(
                    TransactionId,
                    CurrentTransfer.TransactionId,
                    ((long)CurrentTransfer.CashableAmount).CentsToMillicents(),
                    ((long)CurrentTransfer.NonRestrictedAmount).CentsToMillicents(),
                    ((long)CurrentTransfer.RestrictedAmount).CentsToMillicents(),
                    AllowPartialTransfer);
            }

            if (!accepted)
            {
                ReleaseTransactionId();
                ResetCashOutRequestState();
                TerminateLock();
            }

            if (transferAmounts.ContainsKey(AccountType.Cashable))
            {
                CurrentTransfer.CashableAmount = transferAmounts[AccountType.Cashable];
            }

            if (transferAmounts.ContainsKey(AccountType.Promo))
            {
                CurrentTransfer.NonRestrictedAmount = transferAmounts[AccountType.Promo];
            }

            if (transferAmounts.ContainsKey(AccountType.NonCash))
            {
                CurrentTransfer.RestrictedAmount = transferAmounts[AccountType.NonCash];
            }

            Logger.Debug(
                $"Done with Aft Off request -- cash-able={CurrentTransfer.CashableAmount} restricted={CurrentTransfer.RestrictedAmount} non-restricted={CurrentTransfer.NonRestrictedAmount} accepted={accepted}");

            return accepted;
        }
    }
}