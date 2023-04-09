namespace Aristocrat.Monaco.Accounting.HandCount
{
    using Application.Contracts;
    using Contracts;
    using Contracts.Transactions;
    using Contracts.TransferOut;
    using Contracts.HandCount;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xceed.Wpf.Toolkit;
    using Aristocrat.Monaco.Hardware.Contracts.HardMeter;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using System.Linq;
    using Aristocrat.Monaco.Hardware.Contracts.Button;

    /// <summary>
    ///     An <see cref="ITransferOutProvider" /> 
    /// </summary>
    [CLSCompliant(false)]
    public class HardMeterOutProvider : TransferOutProviderBase, IHardMeterOutProvider
    {
        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly IIdProvider _idProvider;
        private readonly IMeterManager _meters;
        private readonly IHardMeter _hardMeter;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionHistory _transactions;
        private readonly IHandCountService _handCountService;
        private readonly ICashOutAmountCalculator _cashOutAmountCalculator;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly int _cashOutHardMeterId;

        public HardMeterOutProvider()
            : this(
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IHardMeter>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IHandCountService>(),
                ServiceManager.GetInstance().GetService<ICashOutAmountCalculator>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IIdProvider>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>()
                )
        {
        }

        public HardMeterOutProvider(
            IBank bank,
            ITransactionHistory transactions,
            IMeterManager meters,
            IHardMeter hardMeter,
            IPersistentStorageManager storage,
            IEventBus bus,
            IHandCountService handCountService,
            ICashOutAmountCalculator cashOutAmountCalculator,
            IPropertiesManager properties,
            IIdProvider idProvider,
            ISystemDisableManager systemDisableManager
            )
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _hardMeter = hardMeter ?? throw new ArgumentNullException(nameof(hardMeter));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
            _cashOutAmountCalculator = cashOutAmountCalculator ??
                                       throw new ArgumentNullException(nameof(cashOutAmountCalculator));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));

            _cashOutHardMeterId = GetCashOutHardMeterId();
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

        public string Name => typeof(HardMeterOutProvider).ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IHardMeterOutProvider) };

        public bool Active { get; private set; }

        public void Initialize()
        {
        }

        private void CompleteTransaction(HardMeterOutTransaction transaction)
        {
            transaction.HardMeterCompletedDateTime = DateTime.UtcNow;
            transaction.State = HardMeterOutState.Completed;

            _transactions.UpdateTransaction(transaction);
        }


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
                if ((cashableAmount > 0) &&
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

        public bool CanRecover(Guid transactionId)
        {
            return _transactions.RecallTransactions<HardMeterOutTransaction>()
                .Any(
                    t => t.BankTransactionId == transactionId && t.State == HardMeterOutState.PendingHardMeterComplete);
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

            _bus.Subscribe<DownEvent>(
                this,
                _ =>
                {
                    keyOff.TrySetResult(null);
                },
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);

            _bus.Subscribe<HardMeterTickStoppedEvent>(
                this,
                _ =>
                {
                    keyOff.TrySetResult(null);
                },
                evt => evt.LogicalId == _cashOutHardMeterId);

            _systemDisableManager.Disable(
                ApplicationConstants.PrintingTicketDisableKey,
                SystemDisablePriority.Immediate,
                () => "PRINTING TICKET...",
                true,
                () => "PRINTING TICKET...");

            return keyOff;
        }

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
                if (transaction.State == HardMeterOutState.PendingHardMeterComplete)
                {
                    await Lockup(transaction, true);

                    return await Task.FromResult(true);
                }
            }

            // There is nothing to recover
            return await Task.FromResult(false);
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

                        transaction.State = HardMeterOutState.PendingHardMeterComplete;

                        _transactions.AddTransaction(transaction);

                        var handCount = _cashOutAmountCalculator.GetHandCountUsed(amount);
                        _handCountService.DecreaseHandCount(handCount);

                        UpdateMeters(transaction, amount);

                        scope.Complete();
                    }
                }
                catch (BankException ex)
                {
                    Logger.Fatal($"Failed to debit the bank: {transaction}", ex);

#if !(RETAIL)
                    _bus.Publish(new LegitimacyLockUpEvent());
#endif
                    throw new TransferOutException("Failed to debit the bank: {transaction}", ex);
                }

                Logger.Debug("Preparing to publish HardMeterOutIssuedEvent");
                _bus.Publish(new HardMeterOutIssuedEvent(transaction));
                Logger.Info($"Hard meter out issued: {transaction}");

                return Task.CompletedTask;
            }


        }

        private void UpdateMeters(HardMeterOutTransaction transaction, long amount)
        {
            Logger.Debug("Entering UpdateMeters");
            if (amount > 0)
            {
                _meters.GetMeter(AccountingMeters.HardMeterOutAmount).Increment(amount);
                _meters.GetMeter(AccountingMeters.HardMeterOutCount).Increment(1);
            }
            Logger.Debug("Finished UpdateMeters");
        }
    }
}