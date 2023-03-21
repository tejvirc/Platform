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
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionHistory _transactions;
        private readonly IHandCountService _handCountService;
        private readonly ICashOutAmountCalculator _cashOutAmountCalculator;
        private readonly ITransferOutExtension _transferOutExtension;

        public HardMeterOutProvider()
            : this(
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IHandCountService>(),
                ServiceManager.GetInstance().GetService<ICashOutAmountCalculator>(),
                ServiceManager.GetInstance().GetService<ITransferOutExtension>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IIdProvider>())
        {
        }

        public HardMeterOutProvider(
            IBank bank,
            ITransactionHistory transactions,
            IMeterManager meters,
            IPersistentStorageManager storage,
            IEventBus bus,
            IHandCountService handCountService,
            ICashOutAmountCalculator cashOutAmountCalculator,
            ITransferOutExtension transferOutExtension,
            IPropertiesManager properties,
            IIdProvider idProvider)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
            _cashOutAmountCalculator = cashOutAmountCalculator ?? throw new ArgumentNullException(nameof(cashOutAmountCalculator));
            _transferOutExtension = transferOutExtension ?? throw new ArgumentNullException(nameof(transferOutExtension));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
        }

        public string Name => typeof(HardMeterOutProvider).ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IHardMeterOutProvider) };

        public bool Active { get; private set; }

        public void Initialize()
        {
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
                var cashOutAmount = _transferOutExtension.PreProcessor(cashableAmount);
                if ((cashOutAmount > 0) &&
                    await Transfer(
                        transactionId,
                        cashOutAmount,
                        associatedTransactions,
                        reason,
                        traceId,
                        cancellationToken))
                {
                    transferredCashable = cashOutAmount;
                }
            }
            finally
            {
                Active = false;
            }

            return new TransferResult(transferredCashable, transferredPromo, transferredNonCash);
        }

        public bool CanRecover(Guid transactionId) => false;

        public async Task<bool> Recover(Guid transactionId, CancellationToken cancellationToken)
        {
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

            var transaction = new HandCountTransaction(transactionId, DateTime.Now, amount, reason)
            {
                TraceId = traceId
            };

            await Commit();

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
                        transaction.LogSequence = _idProvider.GetNextLogSequence<HandCountTransaction>();

                        //_transactions.AddTransaction(transaction);

                        _transferOutExtension.PosProcessor(amount);


                        Logger.Debug("Entering UpdateMeters");

                        UpdateMeters(transaction, amount);
                        Logger.Debug("Finished UpdateMeters");

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

                Logger.Debug("Preparing to publish VoucherIssuedEvent");

                //_bus.Publish(new VoucherIssuedEvent(transaction, ticket));
                //Logger.Info($"Voucher issued: {transaction}");

                return Task.CompletedTask;
            }
        }

        private void UpdateMeters(HandCountTransaction transaction, long amount)
        {
            if (amount > 0)
            {
                _meters.GetMeter(AccountingMeters.VoucherOutCashableAmount).Increment(amount);
                _meters.GetMeter(AccountingMeters.VoucherOutCashableCount).Increment(1);
            }
        }
    }
}