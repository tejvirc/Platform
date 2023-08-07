namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aft;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Aristocrat.Sas.Client.Metering;
    using Base;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using log4net;
    using Protocol.Common.Storage.Entity;
    using Storage;
    using Storage.Models;
    using Storage.Repository;
    using Ticketing;
    using IPrinter = Hardware.Contracts.Printer.IPrinter;

    /// <summary>
    ///     Provides common data used by the Aft classes
    /// </summary>
    public class AftTransferProvider : IAftTransferProvider, IDisposable
    {
        private const double AftCompleteResetTime = 15000.0;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IBank _bank;
        private readonly IAftLockHandler _aftLockHandler;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IMeterManager _meterManager;
        private readonly IAftHistoryBuffer _aftHistory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IStorageDataProvider<AftTransferOptions> _currentTransferProvider;
        private readonly ISasBonusCallback _bonus;
        private readonly IAftOffTransferProvider _aftOffTransferProvider;
        private readonly IAftOnTransferProvider _aftOnTransferProvider;
        private readonly ITicketingCoordinator _ticketingCoordinator;
        private readonly IAftRegistrationProvider _registrationProvider;
		private readonly ITime _time;
		private readonly IMoneyLaunderingMonitor _launderingMonitor;
		private readonly IPlayerBank _playerBank;
        private readonly SasExceptionTimer _sasExceptionTimer;
        private bool _disposed;
        private const byte LastHistoryLogEntryIndex = 0xFF;

        /// <summary>
        ///     Initiates an instance of the AftTransferProvider class
        /// </summary>
        /// <param name="aftLockHandler">reference to the AftLockHandler</param>
        /// <param name="bank">reference to the Bank</param>
        /// <param name="exceptionHandler">reference to the exception handler</param>
        /// <param name="propertiesManager">reference to the PropertiesManager</param>
        /// <param name="meterManager">a reference to the MeterManager</param>
        /// <param name="aftOffTransferProvider">the aft off transfer provider</param>
        /// <param name="aftOnTransferProvider">A reference to the AftOnTransferProvider</param>
        /// <param name="ticketingCoordinator">the ticketing coordinator</param>
        /// <param name="registrationProvider"></param>
        /// <param name="aftHistory">reference to the AftHistoryBuffer</param>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="currentTransferProvider"></param>
        /// <param name="bonus">the bonus handler</param>
        /// <param name="time">reference to the Time service</param>
        /// <param name="launderingMonitor"></param>
        /// <param name="playerBank"></param>
        public AftTransferProvider(
            IAftLockHandler aftLockHandler,
            IBank bank,
            ISasExceptionHandler exceptionHandler,
            IPropertiesManager propertiesManager,
            IMeterManager meterManager,
            IAftOffTransferProvider aftOffTransferProvider,
            IAftOnTransferProvider aftOnTransferProvider,
            ITicketingCoordinator ticketingCoordinator,
            IAftRegistrationProvider registrationProvider,
            IAftHistoryBuffer aftHistory,
            IUnitOfWorkFactory unitOfWorkFactory,
            IStorageDataProvider<AftTransferOptions> currentTransferProvider,
            ISasBonusCallback bonus,
            ITime time,
            IMoneyLaunderingMonitor launderingMonitor,
            IPlayerBank playerBank)
        {
            if (exceptionHandler == null)
            {
                throw new ArgumentNullException(nameof(exceptionHandler));
            }

            _aftLockHandler = aftLockHandler ?? throw new ArgumentNullException(nameof(aftLockHandler));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _aftHistory = aftHistory ?? throw new ArgumentNullException(nameof(aftHistory));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _currentTransferProvider = currentTransferProvider ?? throw new ArgumentNullException(nameof(currentTransferProvider));
            _bonus = bonus ?? throw new ArgumentNullException(nameof(bonus));
            _aftOffTransferProvider =
                aftOffTransferProvider ?? throw new ArgumentNullException(nameof(aftOffTransferProvider));
            _aftOnTransferProvider =
                aftOnTransferProvider ?? throw new ArgumentNullException(nameof(aftOnTransferProvider));
            _ticketingCoordinator = ticketingCoordinator ?? throw new ArgumentNullException(nameof(ticketingCoordinator));
            _registrationProvider = registrationProvider ?? throw new ArgumentNullException(nameof(registrationProvider));
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _launderingMonitor = launderingMonitor ?? throw new ArgumentNullException(nameof(launderingMonitor));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));

            _sasExceptionTimer = new SasExceptionTimer(
                exceptionHandler,
                () => AftCompleteException,
                () => !IsTransferAcknowledgedByHost,
                AftCompleteResetTime);

            Recover();
        }

        /// <inheritdoc />
        public uint AssetNumber =>
            _propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);

        /// <inheritdoc />
        public byte[] RegistrationKey => _registrationProvider.AftRegistrationKey;

        /// <inheritdoc />
        public AftResponseData CurrentTransfer { get; set; } = new AftResponseData
        {
            ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested,
            TransferStatus = AftTransferStatusCode.NoTransferInfoAvailable
        };

        /// <inheritdoc />
        public ulong TransferLimitAmount => (ulong)_propertiesManager
            .GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).TransferLimit;

        /// <inheritdoc />
        public ulong TransferAmount { get; set; }

        /// <inheritdoc />
        public bool TransferFailure { get; set; }

        /// <inheritdoc />
        public bool IsTransferAcknowledgedByHost { get; set; } = true;

        /// <inheritdoc />
        public bool IsTransferInProgress => CurrentTransfer != null && !IsTransferAcknowledgedByHost;

        /// <inheritdoc />
        public bool TransactionIdValid { get; set; }

        /// <inheritdoc />
        public bool TransactionIdUnique { get; set; }

        /// <inheritdoc />
        public bool IsPrinterAvailable
        {
            get
            {
                var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
                return printer?.CanPrint ?? false;
            }
        }

        /// <inheritdoc />
        public bool IsLocked => _aftLockHandler.LockStatus == AftGameLockStatus.GameLocked;

        /// <inheritdoc />
        public bool IsRegistrationKeyAllZeros => CurrentTransfer.RegistrationKey.All(x => x == 0);

        /// <inheritdoc />
        public bool PartialTransfersAllowed =>
            CurrentTransfer.TransferCode == AftTransferCode.TransferRequestPartialTransferAllowed &&
            _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).PartialTransferAllowed;

        /// <inheritdoc />>
        public bool FullTransferRequested =>
            CurrentTransfer.TransferCode == AftTransferCode.TransferRequestFullTransferOnly;

        /// <inheritdoc />
        public bool DebitTransfer => CurrentTransfer.TransferType == AftTransferType.HostToGameDebit ||
                                     CurrentTransfer.TransferType == AftTransferType.HostToGameDebitTicket;

        /// <inheritdoc />
        public bool TransferOff => CurrentTransfer.TransferType == AftTransferType.GameToHostInHouse ||
                                   CurrentTransfer.TransferType == AftTransferType.GameToHostInHouseWin;
        /// <inheritdoc />
        public bool MissingRequiredReceiptFields =>
             IsTransferFlagSet(AftTransferFlags.TransactionReceiptRequested) &&
             (string.IsNullOrEmpty(CurrentTransfer.ReceiptData.PatronAccount) && !DebitTransfer ||
             string.IsNullOrEmpty(CurrentTransfer.ReceiptData.DebitCardNumber) && DebitTransfer);

        /// <inheritdoc />
        public bool PosIdZero => _registrationProvider.PosId == 0;

        /// <inheritdoc />
        public ulong CurrentBankBalanceInCents => (ulong)_bank.QueryBalance().MillicentsToCents();

        /// <inheritdoc />
        public void OnSasInitialized()
        {
            if (IsTransferAcknowledgedByHost)
            {
                return;
            }

            if (CurrentTransfer.TransferStatus != AftTransferStatusCode.TransferPending)
            {
                AftCompleteException = GeneralExceptionCode.AftTransferComplete;
                _sasExceptionTimer.StartTimer();
            }
            else if (IsAftOnRequest && !_aftOnTransferProvider.Recover(CurrentTransfer.TransactionId) ||
                     IsAftOffRequest && !_aftOffTransferProvider.Recover(CurrentTransfer.TransactionId) ||
                     IsBonusRequest && !_bonus.Recover(CurrentTransfer.TransactionId))
            {
                CurrentTransfer.TransferStatus = AftTransferStatusCode.UnexpectedError;
                SetCurrentTransferFailed();
                AftCompleteException = GeneralExceptionCode.AftTransferComplete;
                _sasExceptionTimer.StartTimer();
            }
            else
            {
                Logger.Warn(
                    $"Attempting to recover with an unhandled transfer type {CurrentTransfer.TransferCode} with a status of {CurrentTransfer.TransferStatus}");
            }
        }

        /// <inheritdoc />
        public ulong CurrentBankBalanceInCentsForAccount(AccountType account)
        {
            return (ulong)_bank.QueryBalance(account).MillicentsToCents();
        }

        /// <inheritdoc />
        public bool TransferFundsRequest(AftTransferData data) =>
            data.TransferCode == AftTransferCode.TransferRequestPartialTransferAllowed ||
            data.TransferCode == AftTransferCode.TransferRequestFullTransferOnly;

        /// <inheritdoc />
        public void CheckTransactionId(string transactionId)
        {
            TransactionIdValid = !(transactionId.Length < 1 || transactionId.Length > 20);

            // transactionId can't contain a control character
            if (transactionId.Any(char.IsControl))
            {
                TransactionIdValid = false;
            }

            // check history log entries for the last non-zero amount transfer and ensure
            // that the transaction id for that transfer doesn't match this one
            TransactionIdUnique = LastTransactionIdDifferentInHistoryLog(transactionId);

            Logger.Debug($"CheckTransaction returning validId={TransactionIdValid}, uniqueId={TransactionIdUnique}");
        }

        /// <inheritdoc />
        public void TransferFails(AftTransferStatusCode reason)
        {
            CurrentTransfer.TransferStatus = reason;
            TransferFailure = true;
            UpdatePersistence().FireAndForget();
        }

        /// <inheritdoc />
        public bool IsTransferFlagSet(AftTransferFlags flag)
        {
            return (CurrentTransfer.TransferFlags & flag) != 0;
        }

        /// <inheritdoc />
        public bool LastTransactionIdDifferentInHistoryLog(string transactionId)
        {
            // look up last non-zero entry in the AFT history log
            var lastTransaction = _aftHistory.GetHistoryEntry(LastHistoryLogEntryIndex);

            if (lastTransaction.TransactionId is null)
            {
                Logger.Debug("Not a transaction to compare ids to");
                return true;
            }

            var equal = string.CompareOrdinal(transactionId, lastTransaction.TransactionId) == 0;
            Logger.Debug($"last transaction id is '{lastTransaction.TransactionId}'\ncurrent transaction id is '{transactionId}' strings are equal is {equal}");
            return !equal;
        }

        /// <inheritdoc />
        public void CreateNewTransactionHistoryEntry()
        {
            using (var work = _unitOfWorkFactory.Create())
            {
                work.BeginTransaction(IsolationLevel.Serializable);
                AcknowledgeTransaction();

                // has a transfer completed but not been added to the history buffer?
                if (IsTransactionSuccessFull)
                {
                    // if the non-zero amount transfer has completed successfully then store it in the transfer history buffer
                    // and update the TransactionIndex with the actual buffer position
                    if (TransferAmount > 0 && CurrentTransfer.TransactionIndex == _aftHistory.CurrentBufferIndex)
                    {
                        _aftHistory.AddEntry(CurrentTransfer, work);
                    }
                }

                if (CurrentTransfer?.TransferStatus != AftTransferStatusCode.TransferPending)
                {
                    // We only set the acknowledged transfer flag if we are not pending a current transfer
                    IsTransferAcknowledgedByHost = true;
                }

                var aftCurrentTransfer = _currentTransferProvider.GetData();
                aftCurrentTransfer.CurrentTransfer = StorageHelpers.Serialize(CurrentTransfer);
                aftCurrentTransfer.IsTransferAcknowledgedByHost = IsTransferAcknowledgedByHost;
                _currentTransferProvider.Save(aftCurrentTransfer, work);
                work.Commit();
            }

            AftCompleteException = null;
            _sasExceptionTimer.StopTimer();
        }

        /// <inheritdoc />
        public void CheckForErrorConditions(Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> errorConditions)
        {
            Logger.Debug("checking for error conditions");
            TransferFailure = false;
            foreach (var kvp in errorConditions)
            {
                if (kvp.Key())
                {
                    TransferFails(kvp.Value.code);
                    Logger.Debug($"Error check failed due to {kvp.Value.message}");
                    break;
                }
            }

            if (!TransferFailure)
            {
                Logger.Debug("No error conditions found");
            }
        }

        /// <inheritdoc />
        public void UpdateHostCashoutFlags(AftTransferData data)
        {
            var aftTransferOptions = _currentTransferProvider.GetData();
            var hostCashOutFlags = aftTransferOptions.CurrentTransferFlags &
                                   AftTransferFlags.HostCashOutOptions;

            // Clear host cash out data. we will set current or updated flags
            CurrentTransfer.TransferFlags &= ~AftTransferFlags.HostCashOutOptions;

            if (!TransferFailure && // If failed the transfer we can't update
                (data.TransferFlags & AftTransferFlags.HostCashOutEnableControl) != 0 &&  // We only update if the host told use to enable control
                (hostCashOutFlags != (data.TransferFlags & AftTransferFlags.HostCashOutOptions))) // No need to change anything if we both match
            {
                var updatedFlags = data.TransferFlags & AftTransferFlags.HostCashOutOptions;
                CurrentTransfer.TransferFlags |= updatedFlags;

                aftTransferOptions.CurrentTransferFlags = updatedFlags;
                _currentTransferProvider.Save(aftTransferOptions).FireAndForget();
            }
            else
            {
                CurrentTransfer.TransferFlags |= hostCashOutFlags;
            }
        }

        /// <inheritdoc />
        public async Task DoAftOff()
        {
            CurrentTransfer.PoolId = (ushort)(CurrentTransfer.RestrictedAmount > 0
                ? _ticketingCoordinator.GetData().PoolId
                : SasConstants.NoPoolIdSet);

            // We need to persist before trying the transfer so we can respond correctly if we do not start
            await UpdatePersistence();

            var thresholdReached = _launderingMonitor.IsThresholdReached();

            if (thresholdReached || !_aftOffTransferProvider.AftOffRequest(
                AftData.AftDataFromAftResponseData(CurrentTransfer),
                PartialTransfersAllowed))
            {
                TransferFails(AftTransferStatusCode.GamingMachineUnableToPerformTransfer);
                SetCurrentTransferFailed();
                await UpdatePersistence();
                AftCompleteException = GeneralExceptionCode.AftTransferComplete;
                _sasExceptionTimer.StartTimer();
            }

            if (thresholdReached)
            {
                Logger.Info($"WatOff transfer was redirected to cash-out due to excessive threshold reached.");
                _playerBank.CashOut(true);
            }
        }

        /// <inheritdoc />
        public async Task DoAftOn()
        {
            Logger.Debug($"calling AftOnRequest with status '{CurrentTransfer.TransferStatus}'");

            // We need to persist before trying the transfer so we can respond correctly if we do not start
            await UpdatePersistence();
            if (!_aftOnTransferProvider.AftOnRequest(
                AftData.AftDataFromAftResponseData(CurrentTransfer),
                PartialTransfersAllowed))
            {
                TransferFails(AftTransferStatusCode.GamingMachineUnableToPerformTransfer);
                SetCurrentTransferFailed();
                await UpdatePersistence();
                AftCompleteException = GeneralExceptionCode.AftTransferComplete;
                _sasExceptionTimer.StartTimer();
            }
        }

        /// <inheritdoc />
        public async Task DoBonus()
        {
            await UpdatePersistence();
            var bonusResult = _bonus.AwardAftBonus(AftData.AftDataFromAftResponseData(CurrentTransfer));
            if (bonusResult != AftTransferStatusCode.TransferPending)
            {
                if (bonusResult != AftTransferStatusCode.FullTransferSuccessful)
                {
                    TransferFails(bonusResult);
                    SetCurrentTransferFailed();
                }
                else
                {
                    CurrentTransfer.TransferStatus = bonusResult;
                }

                await UpdatePersistence();
                AftCompleteException = GeneralExceptionCode.AftTransferComplete;
                _sasExceptionTimer.StartTimer();
            }
        }

        /// <inheritdoc />
        public Task DoAftToTicket()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void UpdateFinalAftResponseData(AftData data)
        {
            var updatingStatus = CurrentTransfer.TransferStatus;
            if (CurrentTransfer.TransferStatus == AftTransferStatusCode.TransferPending)
            {
                updatingStatus = FullTransferRequested
                    ? AftTransferStatusCode.FullTransferSuccessful
                    : AftTransferStatusCode.PartialTransferSuccessful;
            }

            UpdateFinalAftResponseData(data, updatingStatus, false);
        }

        /// <inheritdoc />
        public void UpdateFinalAftResponseData(AftData data, AftTransferStatusCode statusCode, bool failed)
        {
            data.UpdateAftResponseData(CurrentTransfer);
            TransferAmount = CurrentTransfer.CashableAmount + CurrentTransfer.RestrictedAmount +
                             CurrentTransfer.NonRestrictedAmount;
            AddCumulativeBalances();

            CurrentTransfer.TransferStatus = statusCode;
            using (var work = _unitOfWorkFactory.Create())
            {
                work.BeginTransaction(IsolationLevel.Serializable);
                if (IsAftOnRequest && CurrentTransfer.RestrictedAmount > 0)
                {
                    var ticketStorageData = _ticketingCoordinator.GetData();
                    ticketStorageData.SetRestrictedExpirationWithPriority(
                        (int)(CurrentTransfer.Expiration == 0 ? _ticketingCoordinator.DefaultTicketExpirationRestricted : CurrentTransfer.Expiration),
                        ticketStorageData.GetHighestPriorityExpiration(),
                        _bank.QueryBalance(AccountType.NonCash).MillicentsToCents() - (long)CurrentTransfer.RestrictedAmount);
                    CurrentTransfer.Expiration = (uint)ticketStorageData.GetHighestPriorityExpiration();
                    ticketStorageData.PoolId = CurrentTransfer.PoolId;
                    _ticketingCoordinator.Save(ticketStorageData, work);
                }
                else
                {
                    CurrentTransfer.Expiration = 0;
                    CurrentTransfer.PoolId = 0;
                }

                if (failed)
                {
                    SetCurrentTransferFailed();
                }

                var aftCurrentTransfer = _currentTransferProvider.GetData();
                aftCurrentTransfer.CurrentTransfer = StorageHelpers.Serialize(CurrentTransfer);
                aftCurrentTransfer.IsTransferAcknowledgedByHost = IsTransferAcknowledgedByHost;
                _currentTransferProvider.Save(aftCurrentTransfer, work);
                work.Commit();
            }

            AftCompleteException = GeneralExceptionCode.AftTransferComplete;
            _sasExceptionTimer.StartTimer(true);
        }

        /// <inheritdoc />
        public void AftTransferFailed()
        {
            SetCurrentTransferFailed();
            IsTransferAcknowledgedByHost = true;
            var aftCurrentTransfer = _currentTransferProvider.GetData();
            aftCurrentTransfer.CurrentTransfer = StorageHelpers.Serialize(CurrentTransfer);
            aftCurrentTransfer.IsTransferAcknowledgedByHost = IsTransferAcknowledgedByHost;
            _currentTransferProvider.Save(aftCurrentTransfer).FireAndForget();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes unmanaged resources
        /// </summary>
        /// <param name="disposing">Whether or not to dispose the object</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _sasExceptionTimer.Dispose();
            }

            _disposed = true;
        }

        private GeneralExceptionCode? AftCompleteException { get; set; }

        private bool IsAftOnRequest => CurrentTransfer?.TransferType == AftTransferType.HostToGameInHouse;

        private bool IsAftOffRequest => CurrentTransfer?.TransferType == AftTransferType.GameToHostInHouse ||
                                        CurrentTransfer?.TransferType == AftTransferType.GameToHostInHouseWin;

        private bool IsBonusRequest => CurrentTransfer?.TransferType == AftTransferType.HostToGameBonusCoinOut ||
                                       CurrentTransfer?.TransferType == AftTransferType.HostToGameBonusJackpot;

        private bool IsTransactionSuccessFull =>
            CurrentTransfer?.TransferStatus == AftTransferStatusCode.FullTransferSuccessful ||
            CurrentTransfer?.TransferStatus == AftTransferStatusCode.PartialTransferSuccessful;

        private void AcknowledgeTransaction()
        {
            if (IsAftOffRequest)
            {
                _aftOffTransferProvider.AcknowledgeTransfer(CurrentTransfer?.TransactionId);
            }
            else if (IsAftOnRequest)
            {
                _aftOnTransferProvider.AcknowledgeTransfer(CurrentTransfer?.TransactionId);
            }
        }

        private void SetCurrentTransferFailed()
        {
            CurrentTransfer.ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested;
            CurrentTransfer.CashableAmount = 0UL;
            CurrentTransfer.RestrictedAmount = 0UL;
            CurrentTransfer.NonRestrictedAmount = 0UL;
            CurrentTransfer.TransactionDateTime = _time.GetLocationTime(DateTime.Now);
        }

        private void AddCumulativeBalances()
        {
            const int aftAccountingDenom = 1; // AFT reports in cents so just provide 1 for the accounting denom
            switch (CurrentTransfer.TransferType)
            {
                // game to host transfers should report meters B8, BA, and BC. See SAS Spec table C-7
                case AftTransferType.GameToHostInHouse:
                case AftTransferType.GameToHostInHouseWin:
                {
                    CurrentTransfer.CumulativeCashableAmount =
                        _meterManager.GetMeterValue(aftAccountingDenom, SasMeterCollection.SasMeterForCode(SasMeterId.AftCashableOut)).MeterValue;
                    CurrentTransfer.CumulativeRestrictedAmount =
                        _meterManager.GetMeterValue(aftAccountingDenom, SasMeterCollection.SasMeterForCode(SasMeterId.AftRestrictedOut)).MeterValue;
                    CurrentTransfer.CumulativeNonRestrictedAmount =
                        _meterManager.GetMeterValue(aftAccountingDenom, SasMeterCollection.SasMeterForCode(SasMeterId.AftNonRestrictedOut)).MeterValue;
                    break;
                }

                // bonus transfers should report meters AE and B0. See SAS Spec table C-7
                case AftTransferType.HostToGameBonusCoinOut:
                case AftTransferType.HostToGameBonusJackpot:
                {
                    CurrentTransfer.CumulativeCashableAmount =
                        _meterManager.GetMeterValue(aftAccountingDenom, SasMeterCollection.SasMeterForCode(SasMeterId.AftCashableBonusIn)).MeterValue;
                    CurrentTransfer.CumulativeRestrictedAmount = 0L;
                    CurrentTransfer.CumulativeNonRestrictedAmount =
                        _meterManager.GetMeterValue(aftAccountingDenom, SasMeterCollection.SasMeterForCode(SasMeterId.AftNonRestrictedBonusIn)).MeterValue;
                    break;
                }

                // all other transfers will report meters A0, A2, and A4. See SAS Spec table C-7
                default:
                {
                    CurrentTransfer.CumulativeCashableAmount =
                        _meterManager.GetMeterValue(aftAccountingDenom, SasMeterCollection.SasMeterForCode(SasMeterId.AftCashableIn)).MeterValue;
                    CurrentTransfer.CumulativeRestrictedAmount =
                        _meterManager.GetMeterValue(aftAccountingDenom, SasMeterCollection.SasMeterForCode(SasMeterId.AftRestrictedIn)).MeterValue;
                    CurrentTransfer.CumulativeNonRestrictedAmount =
                        _meterManager.GetMeterValue(aftAccountingDenom, SasMeterCollection.SasMeterForCode(SasMeterId.AftNonRestrictedIn)).MeterValue;
                    break;
                }
            }
        }

        private async Task UpdatePersistence()
        {
            var aftCurrentTransfer = _currentTransferProvider.GetData();
            aftCurrentTransfer.CurrentTransfer = StorageHelpers.Serialize(CurrentTransfer);
            aftCurrentTransfer.IsTransferAcknowledgedByHost = IsTransferAcknowledgedByHost;
            await _currentTransferProvider.Save(aftCurrentTransfer);
        }

        private void Recover()
        {
            var data = _currentTransferProvider.GetData();
            CurrentTransfer = AftResponseData.FromIAftHistoryLog(
                StorageHelpers.Deserialize(
                    data.CurrentTransfer,
                    () => new AftHistoryLog
                    {
                        ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested,
                        TransferStatus = AftTransferStatusCode.NoTransferInfoAvailable
                    }));

            IsTransferAcknowledgedByHost = data.IsTransferAcknowledgedByHost;
            if (!string.IsNullOrEmpty(data.CurrentTransfer))
            {
                AddCumulativeBalances();
            }
        }
    }
}
