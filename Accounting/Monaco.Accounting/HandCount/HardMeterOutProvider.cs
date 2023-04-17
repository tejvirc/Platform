﻿namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.TransferOut;
    using Contracts.HandCount;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.HardMeter;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     An <see cref="ITransferOutProvider" /> that transfers funds off the EGM by ticking
    ///     a hard meter that is connected to some external device.
    /// </summary>
    [CLSCompliant(false)]
    public class HardMeterOutProvider : TransferOutProviderBase, IHardMeterOutProvider, IDisposable
    {
        private readonly IBank _bank;
        private readonly IEventBus _eventBus;
        private readonly IIdProvider _idProvider;
        private readonly IMeterManager _meters;
        private readonly IHardMeter _hardMeter;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionHistory _transactions;
        private readonly ICashOutAmountCalculator _cashOutAmountCalculator;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly int _cashOutHardMeterId;

        private bool _disposed;

        /// <summary>
        ///     Constructs the provider by retrieving all necessary services from the service manager. This
        ///     constructor is necessary because this is a service in the accounting layer where DI is not used.
        /// </summary>
        public HardMeterOutProvider()
            : this(
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IHardMeter>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ICashOutAmountCalculator>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IIdProvider>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>())
        {
        }

        /// <summary>
        ///     Constructs the provider taking all required services as parameters. For unit testing.
        /// </summary>
        public HardMeterOutProvider(
            IBank bank,
            ITransactionHistory transactions,
            IMeterManager meters,
            IHardMeter hardMeter,
            IPersistentStorageManager storage,
            IEventBus eventBus,
            ICashOutAmountCalculator cashOutAmountCalculator,
            IPropertiesManager properties,
            IIdProvider idProvider,
            ISystemDisableManager systemDisableManager
            )
        {
            _bank = bank
                ?? throw new ArgumentNullException(nameof(bank));
            _transactions = transactions
                ?? throw new ArgumentNullException(nameof(transactions));
            _meters = meters
                ?? throw new ArgumentNullException(nameof(meters));
            _hardMeter = hardMeter
                ?? throw new ArgumentNullException(nameof(hardMeter));
            _storage = storage
                ?? throw new ArgumentNullException(nameof(storage));
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));
            _cashOutAmountCalculator = cashOutAmountCalculator
                ?? throw new ArgumentNullException(nameof(cashOutAmountCalculator));
            _properties = properties
                ?? throw new ArgumentNullException(nameof(properties));
            _idProvider = idProvider
                ?? throw new ArgumentNullException(nameof(idProvider));
            _systemDisableManager = systemDisableManager
                ?? throw new ArgumentNullException(nameof(systemDisableManager));

            _cashOutHardMeterId = GetCashOutHardMeterId();
        }

        /// <inheritdoc />
        public string Name => typeof(HardMeterOutProvider).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHardMeterOutProvider) };

        /// <inheritdoc />
        public bool Active { get; private set; }

        /// <inheritdoc />
        public void Initialize()
        {
            // Nothing to do for this service.
        }

        /// <inheritdoc />
        public async Task<TransferResult> Transfer(
            Guid transactionId,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId,
            CancellationToken cancellationToken)
        {
            var transferredCashable = 0L;
            var transferredPromo = 0L;
            var transferredNonCash = 0L;

            try
            {
                Active = true;
                if (cashableAmount > 0 &&
                    await Transfer(
                        transactionId,
                        cashableAmount,
                        associatedTransactions,
                        reason,
                        traceId,
                        cancellationToken))
                {
                    transferredCashable = cashableAmount;
                }
            }
            finally
            {
                Active = false;
            }

            return new TransferResult(transferredCashable, transferredPromo, transferredNonCash);
        }

        /// <inheritdoc />
        public bool CanRecover(Guid transactionId)
        {
            return _transactions.RecallTransactions<HardMeterOutTransaction>()
                .Any(t => t.BankTransactionId == transactionId && t.State == HardMeterOutState.Pending);
        }

        /// <inheritdoc />
        public async Task<bool> Recover(Guid transactionId, CancellationToken cancellationToken)
        {
            if (Active)
            {
                return false;
            }

            var transaction = _transactions.RecallTransactions<HardMeterOutTransaction>()
                .FirstOrDefault(t => t.BankTransactionId == transactionId);

            Logger.Debug($"Checking hard meter out recovery - {transactionId}");
            if (transaction != null)
            {
                if (transaction.State == HardMeterOutState.Pending)
                {
                    await Lockup(transaction, true);

                    return await Task.FromResult(true);
                }
            }

            // There is nothing to recover
            return await Task.FromResult(false);
        }

        /// <inheritdoc />
        ~HardMeterOutProvider() => Dispose(disposing: false);

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private int GetCashOutHardMeterId()
        {
            var currentMeterMapping = _properties.GetValue(
                ApplicationConstants.HardMeterMapSelectionValue,
                "Default");

            var config = ConfigurationUtilities.GetConfiguration(
                "/HardMeterMapping/Configuration",
                () => new HardMeterMappingConfiguration());

            if (config.HardMeterMapping.Length <= 0)
            {
                return 0;
            }

            var mapping = config.HardMeterMapping.FirstOrDefault((x) => x.Name == currentMeterMapping);

            if (mapping == null)
            {
                mapping = config.HardMeterMapping.FirstOrDefault((x) => x.Default) ?? config.HardMeterMapping.FirstOrDefault();
            }

            if (mapping != null)
            {
                return mapping.HardMeter.First(m => m.SoftMeter.Any(s => s.Name == AccountingMeters.HardMeterOutAmount)).LogicalId;
            }

            return 0;
        }

        private async Task Lockup(HardMeterOutTransaction transaction, bool inRecovery)
        {
            var keyOff = Initiate();

            if (inRecovery)
            {
                if (_hardMeter.GetHardMeterState(_cashOutHardMeterId) != HardMeterState.Enabled || _hardMeter.GetHardMeterValue(_cashOutHardMeterId) > 0)
                    await keyOff.Task;
            }
            else
                await keyOff.Task;

            CompleteTransaction(transaction);
            _systemDisableManager.Enable(ApplicationConstants.PrintingTicketDisableKey);
        }

        private TaskCompletionSource<object> Initiate()
        {
            var keyOff = new TaskCompletionSource<object>();

            _eventBus.Subscribe<DownEvent>(
                this,
                _ =>
                {
                    keyOff.TrySetResult(null);
                },
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);

            _eventBus.Subscribe<HardMeterTickStoppedEvent>(
                this,
                _ =>
                {
                    keyOff.TrySetResult(null);
                },
                evt => evt.LogicalId == _cashOutHardMeterId);

            _systemDisableManager.Disable(
                ApplicationConstants.PrintingTicketDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.PrintingTicket),
                true,
                () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.PrintingTicket));

            return keyOff;
        }

        private async Task<bool> Transfer(
            Guid transactionId,
            long amount,
            IEnumerable<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId,
            CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return await Task.FromResult(false);
            }

            var transaction = new HardMeterOutTransaction(transactionId, DateTime.UtcNow, amount, reason)
            {
                TraceId = traceId
            };

            await Commit();

            await Lockup(transaction, false);

            return await Task.FromResult(true);

            Task Commit()
            {
                Logger.Debug($"Committing the hand count transaction {transaction}");

                try
                {
                    using (var scope = _storage.ScopedTransaction())
                    {
                        if (transaction.Reason.AffectsBalance())
                        {
                            _bank.Withdraw(AccountType.Cashable, amount, transaction.BankTransactionId);
                        }

                        // Unique log sequence number assigned by the EGM; a series that strictly increases by 1 (one) starting at 1 (one).
                        transaction.LogSequence = _idProvider.GetNextLogSequence<HardMeterOutTransaction>();

                        transaction.State = HardMeterOutState.Pending;

                        _transactions.AddTransaction(transaction);

                        _cashOutAmountCalculator.PostProcessTransaction(amount);

                        UpdateMeters(transaction, amount);

                        scope.Complete();
                    }
                }
                catch (BankException ex)
                {
                    Logger.Fatal($"Failed to debit the bank: {transaction}", ex);

#if !(RETAIL)
                    _eventBus.Publish(new LegitimacyLockUpEvent());
#endif
                    throw new TransferOutException("Failed to debit the bank: {transaction}", ex);
                }

                Logger.Info($"Hard meter out issued: {transaction}");

                return Task.CompletedTask;
            }
        }

        private void UpdateMeters(HardMeterOutTransaction transaction, long amount)
        {
            if (amount > 0)
            {
                _meters.GetMeter(AccountingMeters.HardMeterOutAmount).Increment(amount);
                _meters.GetMeter(AccountingMeters.HardMeterOutCount).Increment(1);
            }
        }

        private void CompleteTransaction(HardMeterOutTransaction transaction)
        {
            transaction.HardMeterCompletedDateTime = DateTime.UtcNow;
            transaction.State = HardMeterOutState.Completed;

            _transactions.UpdateTransaction(transaction);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _eventBus.UnsubscribeAll(this);
                }

                _disposed = true;
            }
        }
    }
}