namespace Aristocrat.Monaco.Sas.BonusProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Exceptions;
    using Gaming.Contracts.Bonus;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Storage.Models;

    /// <summary>Definition of the SasBonusProvider class.</summary>
    public sealed class SasBonusProvider : ISasBonusCallback, IDisposable
    {
        private const string LegacyBonusHostTransactionPrefix = "LEGACY";
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ISasHost _sasHost;
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IBonusHandler _bonusHandler;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAftLockHandler _aftLockHandler;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IMeterManager _meterManager;
        private readonly IEventBus _eventBus;
        private readonly IPersistentStorageManager _storage;

        private bool _disposed;

        /// <summary> Constructs the object </summary>
        public SasBonusProvider(
            ISasHost sasHost,
            ISasExceptionHandler exceptionHandler,
            IBonusHandler bonusHandler,
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisableManager,
            IMeterManager meterManager,
            IAftLockHandler aftLockHandler,
            IEventBus eventBus,
            IPersistentStorageManager storage)
        {
            _sasHost = sasHost ?? throw new ArgumentNullException(nameof(sasHost));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _aftLockHandler = aftLockHandler ?? throw new ArgumentNullException(nameof(aftLockHandler));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            _eventBus.Subscribe<BonusAwardedEvent>(this, HandleEvent, IsSasBonusEvent);
            _eventBus.Subscribe<BonusCancelledEvent>(this, HandleEvent, IsSasBonusEvent);
            _eventBus.Subscribe<BonusFailedEvent>(this, HandleEvent, IsSasBonusEvent);
        }

        /// <inheritdoc />
        public void SetGameDelay(uint delayTime)
        {
            // Delay time in units of 100ms
            _bonusHandler.SetGameEndDelay(TimeSpan.FromMilliseconds(delayTime * 10));
        }

        /// <inheritdoc />
        public bool IsAftBonusAllowed(AftData data)
        {
            // Make sure Aft bonus transfers are supported and the system is currently enabled.
            return !_systemDisableManager.IsDisabled
                   && _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).AftBonusAllowed;
        }

        /// <inheritdoc />
        public void AwardLegacyBonus(long amount, TaxStatus taxStatus)
        {
            Logger.Debug($"Received legacy bonus -- amount: {amount}; tax status: {taxStatus}");

            PopulateLegacyBonusInfo(amount, taxStatus);
        }

        /// <inheritdoc />
        public AftTransferStatusCode AwardAftBonus(AftData data)
        {
            if (data == null)
            {
                return AftTransferStatusCode.GamingMachineUnableToPerformTransfer;
            }

            Logger.Debug($"Received Aft bonus -- cash-able amount: {data.CashableAmount} non-restricted amount: {data.NonRestrictedAmount}");

            PayMethod payMethod;
            switch (data.TransferType)
            {
                case AftTransferType.HostToGameBonusCoinOut:
                    payMethod = PayMethod.Any;
                    break;
                case AftTransferType.HostToGameBonusJackpot:
                    payMethod = PayMethod.Handpay;
                    break;
                case AftTransferType.HostToGameInHouseTicket:
                    payMethod = PayMethod.Voucher;
                    break;
                case AftTransferType.TransferUnknown:
                    payMethod = PayMethod.Any;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid Aft transfer type: {data.TransferType}");
            }

            var amounts = GetBonusDictionary(data);

            // Remove the lock here we will get it below if we can
            TerminateAftLock();
            var transaction = _bonusHandler.Award(
                new StandardBonus(
                    data.TransactionId,
                    amounts[AccountType.Cashable].CentsToMillicents(),
                    amounts[AccountType.NonCash].CentsToMillicents(),
                    amounts[AccountType.Promo].CentsToMillicents(),
                    payMethod,
                    protocol: CommsProtocol.SAS));

            if (transaction == null)
            {
                return AftTransferStatusCode.GamingMachineUnableToPerformTransfer;
            }

            return transaction.Exception == 0 ? AftTransferStatusCode.TransferPending : AftTransferStatusCode.GamingMachineUnableToPerformTransfer;
        }

        /// <inheritdoc />
        public bool Recover(string transactionId)
        {
            var transaction = _bonusHandler.Transactions.FirstOrDefault(x => x.BonusId == transactionId);
            if (transaction is null)
            {
                return false;
            }

            switch (transaction.State)
            {
                case BonusState.Committed when transaction.Exception == (int)BonusException.None:
                    HandleEvent(new BonusAwardedEvent(transaction));
                    break;
                case BonusState.Committed when transaction.Exception == (int)BonusException.Cancelled:
                    HandleEvent(new BonusCancelledEvent(transaction));
                    break;
                case BonusState.Committed:
                    HandleEvent(new BonusFailedEvent(transaction));
                    break;
            }

            return true;
        }

        /// <inheritdoc />
        public BonusTransaction GetLastPaidLegacyBonus()
        {
            return _bonusHandler.Transactions.OrderByDescending(t => t.TransactionId).FirstOrDefault(
                x => x.State >= BonusState.Committed && x.BonusId.StartsWith(LegacyBonusHostTransactionPrefix));
        }

        /// <inheritdoc />
        public void AcknowledgeBonus(string bonusId)
        {
            _bonusHandler.Acknowledge(bonusId);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _eventBus.UnsubscribeAll(this);
            _disposed = true;
        }

        private static bool IsSasBonusEvent(BaseBonusEvent bonusEvent) =>
            bonusEvent.Transaction?.Protocol == CommsProtocol.SAS;

        private static Dictionary<AccountType, long> GetBonusDictionary(AftData data)
        {
            ulong[] amounts = { data.CashableAmount, data.RestrictedAmount, data.NonRestrictedAmount };
            AccountType[] accountTypes = { AccountType.Cashable, AccountType.NonCash, AccountType.Promo };

            var dictionary = new Dictionary<AccountType, long>();
            for (var i = 0; i < amounts.Length; i++)
            {
                dictionary.Add(accountTypes[i], (long)amounts[i]);
            }

            return dictionary;
        }

        private void PopulateLegacyBonusInfo(long amount, TaxStatus taxStatus)
        {
            var bonusId = $"{LegacyBonusHostTransactionPrefix}_{taxStatus}_{Guid.NewGuid()}";

            var transaction =
                _bonusHandler.Award(new StandardBonus(
                    bonusId,
                    amount.CentsToMillicents(),
                    0,
                    0,
                    PayMethod.Any,
                    true,
                    taxStatus.GetBonusMode(),
                    protocol: CommsProtocol.SAS));

            if (transaction != null)
            {
                Logger.Debug($"Queued legacy bonus request amount={amount} taxStatus={taxStatus} bonusId={bonusId}");
            }
            else
            {
                Logger.Warn("Cannot escrow legacy bonus.");
            }
        }

        private void HandleEvent(BonusFailedEvent evt)
        {
            if (evt.Transaction.BonusId.StartsWith(LegacyBonusHostTransactionPrefix))
            {
                return;
            }

            var aftData = BonusTransactionToAftData(evt.Transaction);
            aftData.TransferStatus = AftTransferStatusCode.GamingMachineUnableToPerformTransfer;
            _sasHost.AftTransferFailed(aftData, aftData.TransferStatus);
        }

        private void HandleEvent(BonusCancelledEvent evt)
        {
            if (evt.Transaction.BonusId.StartsWith(LegacyBonusHostTransactionPrefix))
            {
                return;
            }

            var aftData = BonusTransactionToAftData(evt.Transaction);
            aftData.TransferStatus = AftTransferStatusCode.TransferCanceledByHost;
            _sasHost.AftTransferFailed(aftData, aftData.TransferStatus);
        }

        private void HandleEvent(BonusAwardedEvent evt)
        {
            if (evt.Transaction.BonusId.StartsWith(LegacyBonusHostTransactionPrefix))
            {
                var denoms = _propertiesManager.GetValue(SasProperties.SasHosts, Enumerable.Empty<Host>())
                    .Select(x => x.AccountingDenom).ToList();
                _exceptionHandler.ReportException(
                    client => GetLegacyBonusAwardedException(evt.Transaction, denoms, client),
                    GeneralExceptionCode.LegacyBonusPayAwarded);
            }
            else
            {
                var aftData = BonusTransactionToAftData(evt.Transaction);
                _sasHost.AftBonusAwarded(aftData);

                using (var scope = _storage.ScopedTransaction())
                {
                    if (evt.Transaction.CashableAmount > 0)
                    {
                        _meterManager.GetMeter(Contracts.Metering.SasMeterNames.AftCashableBonusIn).Increment(
                            evt.Transaction.CashableAmount);
                        _meterManager.GetMeter(Contracts.Metering.SasMeterNames.AftCashableBonusInQuantity)
                            .Increment(1);
                    }

                    if (evt.Transaction.PromoAmount > 0)
                    {
                        _meterManager.GetMeter(Contracts.Metering.SasMeterNames.AftNonRestrictedBonusIn).Increment(
                            evt.Transaction.PromoAmount);
                        _meterManager.GetMeter(Contracts.Metering.SasMeterNames.AftNonRestrictedBonusInQuantity)
                            .Increment(1);
                    }

                    scope.Complete();
                }

                _sasHost.AftTransferComplete(aftData);
            }
        }

        private static LegacyBonusAwardedExceptionBuilder GetLegacyBonusAwardedException(
            BonusTransaction transaction,
            IReadOnlyList<long> accountingDenoms,
            byte clientNumber)
        {
            return accountingDenoms.Count <= clientNumber
                ? null
                : new LegacyBonusAwardedExceptionBuilder(
                    transaction.PaidAmount,
                    transaction.Mode.GeTaxStatus(),
                    accountingDenoms[clientNumber]);
        }

        private static AftData BonusTransactionToAftData(BonusTransaction transaction)
        {
            return new AftData
            {
                CashableAmount = (ulong)transaction.CashableAmount,
                RestrictedAmount = (ulong)transaction.NonCashAmount,
                NonRestrictedAmount = (ulong)transaction.PromoAmount,
                ReceiptData = { ReceiptTime = transaction.PaidDateTime },
                TransactionDateTime = transaction.LastUpdate,
                TransactionId = transaction.BonusId
            };
        }

        private void TerminateAftLock()
        {
            _aftLockHandler.AftLock(false, 0);
        }
    }
}
