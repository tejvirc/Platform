namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

    /// <summary>
    ///     Definition of the AftOnTransferProvider class.
    /// </summary>
    public sealed class AftOnTransferProvider :
        AftTransferProviderBase,
        IAftOnTransferProvider,
        IWatTransferOnProvider,
        ITransactionRequestor
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly Guid TransactionOwnerId = new Guid("{0F6C27CE-C5D4-40e0-9896-467526AB2217}");

        private bool _transferAccepted;
        private bool _printReceipt;

        private bool _serviceInitialized;
        private readonly object _mutex = new object();
        private readonly IWatTransferOnHandler _watOnHandler;
        private readonly ITransactionHistory _transactionHistory;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;
        private readonly IAftRegistrationProvider _aftRegistration;
        private readonly IBank _bank;
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Initializes a new instance of the AftOnTransferProvider class.
        /// </summary>
        public AftOnTransferProvider(
            ISasHost sasHost,
            IAftLockHandler aftLock,
            IWatTransferOnHandler watOn,
            ITransactionCoordinator transactionCoordinator,
            IAftRegistrationProvider aftRegistration,
            ITransactionHistory transactionHistory,
            ISystemDisableManager systemDisableManager,
            IOperatorMenuLauncher operatorMenuLauncher,
            ITime timeService,
            IPropertiesManager propertiesManager,
            IBank bank,
            IEventBus eventBus,
            IAutoPlayStatusProvider autoPlayStatusProvider)
            : base(
                aftLock,
                sasHost,
                timeService,
                propertiesManager,
                transactionCoordinator,
                autoPlayStatusProvider)
        {
            _watOnHandler = watOn;
            _transactionHistory = transactionHistory;
            _systemDisableManager = systemDisableManager;
            _operatorMenuLauncher = operatorMenuLauncher;
            _aftRegistration = aftRegistration;
            _bank = bank;
            _eventBus = eventBus;
        }

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IAftOnTransferProvider), typeof(IWatTransferOnProvider) };

        /// <inheritdoc />
        public override void Initialize()
        {
            Logger.Debug("Initializing service...");
            base.Initialize();

            // Check initial state prior to registering with SasHost.
            if (_systemDisableManager.IsDisabled)
            {
                AftState |= AftDisableConditions.SystemDisabled;
            }

            if (_operatorMenuLauncher.IsShowing)
            {
                AftState |= AftDisableConditions.OperatorMenuEntered;
            }

            TransactionCoordinator.AbandonTransactions(RequestorGuid);
            _serviceInitialized = true;
            Logger.Debug("Service initialized");
        }

        /// <inheritdoc />
        public bool IsAftOnAvailable => !IsAftDisabled;

        /// <inheritdoc />
        public bool IsAftPending { get; private set; }

        /// <inheritdoc />
        public Guid RequestorGuid => TransactionOwnerId;

        /// <inheritdoc />
        public bool CanTransfer => !IsAftDisabled;

        /// <inheritdoc />
        public bool InitiateAftOn()
        {
            Logger.Debug("Start of InitiateAftOn().");
            if (!_serviceInitialized)
            {
                Logger.Warn("Client cannot initiate a Aft On before the initialization is done.");
                return false;
            }

            if (IsAftDisabled)
            {
                return false;
            }

            lock (_mutex)
            {
                IsAftPending = false;
                _transferAccepted = false;

                RetrieveTransactionIdFromLock();
                if (TransactionId == Guid.Empty)
                {
                    if (AftLockHandler.LockStatus != AftGameLockStatus.GameLockPending)
                    {
                        // Request a transaction if SasLockHandler hasn't started to request it.
                        TransactionId = TransactionCoordinator.RequestTransaction(this, TransactionType.Write);
                    }
                }

                if (TransactionId != Guid.Empty)
                {
                    _transferAccepted = true;
                }
                else
                {
                    // Pending for the transaction to be available.
                    IsAftPending = true;
                }

                Logger.Debug($"m_transferAccepted={_transferAccepted} IsAftPending={IsAftPending}");
                if (!_transferAccepted && !IsAftPending)
                {
                    ReleaseTransactionId();
                }
            }

            if (!_transferAccepted && !IsAftPending)
            {
                TerminateLock();
            }

            Logger.Debug($"End of InitiateAftOn(): m_transferAccepted={_transferAccepted}");
            return _transferAccepted;
        }

        /// <inheritdoc />
        public void CancelAftOn()
        {
            Logger.Debug("Start of CancelAftOn().");
            if (!_serviceInitialized)
            {
                Logger.Warn("Client cannot cancel a Aft On before the initialization is done.");
                return;
            }

            lock (_mutex)
            {
                _watOnHandler.CancelTransfer(CurrentTransfer.TransactionId, (int)AftTransferStatusCode.TransferCanceledByHost);
                CurrentTransfer = new AftData();
                ReleaseTransactionId();
            }

            TerminateLock();
            Logger.Debug("End of CancelAftOn().");
        }

        /// <inheritdoc />
        public bool AftOnRequest(AftData data, bool partialAllowed)
        {
            if (!_serviceInitialized)
            {
                Logger.Warn("Client cannot request a Aft On before the initialization is done.");
                return false;
            }

            lock (_mutex)
            {
                CurrentTransfer = data;
            }

            if (!InitiateAftOn() && !TransactionPending)
            {
                data.CashableAmount = 0;
                data.RestrictedAmount = 0;
                data.NonRestrictedAmount = 0;
                SasHost.AftTransferFailed(data, AftTransferStatusCode.GamingMachineUnableToPerformTransfer);
                return false;
            }

            lock (_mutex)
            {
                Logger.Debug($"Start of AftOnRequest(). TransactionId is '{TransactionId}'");
                if (TransactionId != Guid.Empty)
                {
                    return PerformAftOnRequest();
                }

                Logger.Debug("End of AftOnRequest().");
                return true;
            }
        }

        /// <inheritdoc />
        public void AftOnRejected()
        {
            if (!_serviceInitialized)
            {
                Logger.Warn("Client cannot reject a Aft On before the initialization is done.");
                return;
            }

            _eventBus.Publish(new WatOnRejectedEvent());
        }

        /// <inheritdoc />
        public bool Recover(string transactionId)
        {
            var watOnTransaction = _transactionHistory.RecallTransactions<WatOnTransaction>()
                .FirstOrDefault(x => x.Status != WatStatus.Complete && x.RequestId == transactionId);
            if (watOnTransaction == null)
            {
                return false;
            }

            switch (watOnTransaction.Status)
            {
                case WatStatus.RequestReceived:
                case WatStatus.CancelReceived:
                case WatStatus.Initiated:
                case WatStatus.Authorized:
                    // Cancel the transaction we should not attempt to recover
                    _watOnHandler.CancelTransfer(watOnTransaction, (int)AftTransferStatusCode.UnexpectedError);
                    break;
                case WatStatus.Committed:
                    HandleTransferComplete(CurrentTransfer, watOnTransaction, Logger);
                    FinishAft(watOnTransaction);
                    break;
            }

            return true;
        }

        /// <inheritdoc />
        public void AcknowledgeTransfer(string transactionId)
        {
            var watOnTransaction = _transactionHistory.RecallTransactions<WatOnTransaction>()
                .FirstOrDefault(x => x.RequestId == transactionId);
            if (watOnTransaction != null)
            {
                _watOnHandler.AcknowledgeTransfer(watOnTransaction);
            }
        }

        /// <inheritdoc />
        public void NotifyTransactionReady(Guid requestId)
        {
            Logger.Debug("NotifyTransactionReady");
            Task.Run(() => OnTransactionReady(requestId));
        }

        /// <inheritdoc />
        public override void OnStateChanged()
        {
            if (!_serviceInitialized)
            {
                Logger.Debug("state is changed before the initialization is done. Not an issue!");
                return;
            }

            lock (_mutex)
            {
                AftDisabledByEvent(!IsAftDisabled);

                if (IsAftPending)
                {
                    Logger.Debug("Calling PerformAftOnRequest from OnStateChanged");
                    PerformAftOnRequest();
                }
            }
        }

        /// <inheritdoc />
        public Task<bool> InitiateTransfer(WatOnTransaction transaction)
        {
            if (!_serviceInitialized)
            {
                Logger.Error("Platform cannot initiate a transfer before the initialization is done.");
                return Task.FromResult(false);
            }

            if (transaction.AllowReducedAmounts)
            {
                var transferLimit = PropertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                    .TransferLimit.CentsToMillicents();
                var currentBalance = _bank.QueryBalance();
                var availableBalance = Math.Min(
                    (currentBalance > _bank.Limit ? 0 : _bank.Limit - currentBalance),
                    transferLimit);
                transaction.AuthorizedNonCashAmount = Math.Min(transaction.NonCashAmount, availableBalance);
                availableBalance -= transaction.AuthorizedNonCashAmount;
                transaction.AuthorizedPromoAmount = Math.Min(transaction.PromoAmount, availableBalance);
                availableBalance -= transaction.AuthorizedPromoAmount;
                transaction.AuthorizedCashableAmount = Math.Min(transaction.CashableAmount, availableBalance);
            }
            else
            {
                transaction.AuthorizedCashableAmount = transaction.CashableAmount;
                transaction.AuthorizedPromoAmount = transaction.PromoAmount;
                transaction.AuthorizedNonCashAmount = transaction.NonCashAmount;
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task CommitTransfer(WatOnTransaction transaction)
        {
            if (!_serviceInitialized)
            {
                Logger.Error("Platform cannot commit a transfer before the initialization is done.");
                return;
            }

            if (!string.IsNullOrEmpty(CurrentTransfer.TransactionId))
            {
                Logger.Debug("Completing transfer on...");
                HandleTransferComplete(CurrentTransfer, transaction, Logger);

                if (_printReceipt)
                {
                    // Update the Transfer Status so that it can be read before the ticket completes
                    FinishTransferStatus(transaction);

                    await PrintReceipt(SasAftReceiptCreator.CreateAftReceipt(CurrentTransfer, _aftRegistration));
                    _printReceipt = false;
                }

                await Task.Run(() => AftOnCommitted(transaction));
            }
            else
            {
                Logger.Error("TransferOffComplete(): invalid transaction!");
            }
        }

        /// <inheritdoc />
        protected override void AftDisabledByEvent(bool isEnabled)
        {
            if (!isEnabled && (AftLockHandler.AftLockTransferConditions &
                               (AftTransferConditions.TransferToGamingMachineOk | AftTransferConditions.BonusAwardToGamingMachineOk)) != AftTransferConditions.None)
            {
                TerminateLock();
            }

            Logger.Debug($"Setting Aft On Enabled state to: {isEnabled}");

            SasHost.SetAftInEnabled(isEnabled);
        }

        /// <inheritdoc />
        protected override void HandleLockAcquired()
        {
            lock (_mutex)
            {
                if (IsAftPending)
                {
                    RetrieveTransactionIdFromLock();

                    Logger.Debug("Calling PerformAftOnRequest from HandleLockAcquired");
                    PerformAftOnRequest();
                }
            }
        }

        private void OnTransactionReady(Guid requestId)
        {
            lock (_mutex)
            {
                TransactionRequestId = requestId;
                TransactionId = TransactionCoordinator.RetrieveTransaction(TransactionRequestId);

                Logger.Debug("Calling PerformAftOnRequest from OnTransactionReady");
                PerformAftOnRequest();
            }
        }

        private bool PerformAftOnRequest()
        {
            Logger.Debug("Start of PerformAftOnRequest().");

            if (string.IsNullOrEmpty(CurrentTransfer.TransactionId) || _systemDisableManager.IsDisabled)
            {
                Logger.Debug("Cannot perform AftOn request.");
                return false;
            }

            IsAftPending = false;

            var transferAmounts = GetTransferAmountsDictionary(CurrentTransfer);

            // If AftData Request Aft Receipt flag is set, set the flag to print a receipt.
            _printReceipt = (CurrentTransfer.TransferFlags & AftTransferFlags.TransactionReceiptRequested) == AftTransferFlags.TransactionReceiptRequested;

            SasHost.SetAftReceiptStatus(_printReceipt ? ReceiptStatus.ReceiptPending : ReceiptStatus.NoReceiptRequested);

            var allowPartial = CurrentTransfer.TransferCode == AftTransferCode.TransferRequestPartialTransferAllowed;
            var accepted = _watOnHandler.RequestTransfer(
                TransactionId,
                CurrentTransfer.TransactionId,
                ((long)CurrentTransfer.CashableAmount).CentsToMillicents(),
                ((long)CurrentTransfer.NonRestrictedAmount).CentsToMillicents(),
                ((long)CurrentTransfer.RestrictedAmount).CentsToMillicents(),
                allowPartial);

            if (!accepted)
            {
                // We need to tell the back-end that an internal error occurs and meanwhile
                // a necessary cleanup is needed for WatOnHandler. Otherwise no new transfer-in
                // will be accepted. 
                Logger.Error("Failed to perform AftOn. Most likely WatOnHandler is in a wrong state.");
                _printReceipt = false;
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
                $"End of PerformAftOnRequest -- cash-able={CurrentTransfer.CashableAmount} restricted={CurrentTransfer.RestrictedAmount} non-restricted={CurrentTransfer.NonRestrictedAmount} accepted={accepted}");

            return accepted;
        }

        private void AftOnCommitted(WatOnTransaction transaction)
        {
            Logger.Debug("AftOnCommitted()");

            if (!_serviceInitialized)
            {
                Logger.Error("Platform cannot commit Aft On before the initialization is done.");
                return;
            }

            ReleaseTransactionId();
            TerminateLock();

            FinishAft(transaction);
        }

        private void FinishTransferStatus(WatOnTransaction transaction)
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

        private void FinishAft(WatOnTransaction transaction)
        {
            FinishTransferStatus(transaction);
        }
    }
}
