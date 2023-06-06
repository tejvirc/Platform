namespace Aristocrat.Monaco.Accounting.HandCount
{
    using Application.Contracts.Metering;
    using Contracts;
    using Contracts.HandCount;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Aristocrat.Monaco.Kernel.Contracts.Events;
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    ///     Definition of the HandCountService class.
    /// </summary>
    public class HandCountService : IHandCountService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Guid RequestorId = new Guid("{755B6E71-B5A1-4E51-9394-B1B9CC298F65}");

        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;
        private readonly IMeter _handCountMeter;
        private readonly IMeter _residualAmountMeter;
        private readonly IPersistentStorageManager _storage;
        private readonly IBank _bank;
        private readonly ITransactionHistory _transactions;
        private readonly ITransactionCoordinator _transactionCoordinator;

        private bool _disposed;
        /// <summary>
        ///     Constructs the service by retrieving all necessary services from the service manager. This
        ///     constructor is necessary because this is a service in the accounting layer where DI is not used.
        /// </summary>
        public HandCountService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<ITransactionCoordinator>()
            )
        {
        }

        /// <summary>
        ///     Constructs the service taking all required services as parameters. For unit testing.
        /// </summary>
        public HandCountService(
            IEventBus eventBus,
            IMeterManager meters,
            IPropertiesManager propertyProvider,
            IPersistentStorageManager storage,
            IBank bank,
            ITransactionHistory transactions,
            ITransactionCoordinator transactionCoordinator
            )
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            if (meters == null)
            {
                throw new ArgumentNullException(nameof(meters));
            }
            _handCountMeter = meters.GetMeter(AccountingMeters.HandCount);
            _residualAmountMeter = meters.GetMeter(AccountingMeters.ResidualAmount);

            _properties = propertyProvider ?? throw new ArgumentNullException(nameof(propertyProvider));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
        }

        public string Name => typeof(HandCountService).FullName;

        public int HandCount => (int)_handCountMeter.GetValue(MeterTimeframe.Lifetime);

        public ICollection<Type> ServiceTypes => new[] { typeof(IHandCountService) };

        /// <inheritdoc />
        public bool HandCountServiceEnabled => (bool)ServiceManager.GetInstance().GetService<IPropertiesManager>()
            .GetProperty(AccountingConstants.HandCountServiceEnabled, false);

        public void Initialize()
        {
            _eventBus.Subscribe<InitializationCompletedEvent>(this, HandleEvent);
        }
        private void HandleEvent(InitializationCompletedEvent obj)
        {
            SendHandCountChangedEvent();
        }

        public void IncrementHandCount()
        {
            _handCountMeter.Increment(1);
            SendHandCountChangedEvent();
            Logger.Info($"IncrementHandCount to {HandCount}");
        }

        public void DecreaseHandCount(int n)
        {
            _handCountMeter.Increment(-n);
            SendHandCountChangedEvent();
            Logger.Info($"DecreaseHandCount by {n} to {HandCount}");
        }

        public void ResetHandCount(long residualAmount)
        {
            try
            {
                if (residualAmount != (long)_properties.GetProperty(PropertyKey.CurrentBalance, 0L))
                {
                    Logger.Info($"ResetHandCount: current balance is different from residualAmount, abandon reset hand count and residual credits");
                    return;
                }

                using (var scope = _storage.ScopedTransaction())
                {
                    if (residualAmount > 0)
                    {
                        var transactionId = _transactionCoordinator.RequestTransaction(RequestorId, 0, TransactionType.Write, true);
                        if (transactionId == Guid.Empty)
                        {
                            return;
                        }

                        var transaction = new ResidualCreditsTransaction(transactionId, DateTime.UtcNow, residualAmount);
                        _transactions.AddTransaction(transaction);
                        _bank.Withdraw(AccountType.Cashable, residualAmount, transaction.BankTransactionId);

                        _transactionCoordinator.ReleaseTransaction(transactionId);
                    }

                    var handCountNeedsReset = HandCount > 0;
                    if (handCountNeedsReset)
                    {
                         DecreaseHandCount(HandCount);
                    }

                    _residualAmountMeter.Increment(residualAmount);

                    scope.Complete();

                    if (handCountNeedsReset)
                    {
                        SendHandCountChangedEvent();
                    }
                }
            }
            catch (BankException ex)
            {
                Logger.Fatal($"Failed to debit the bank", ex);

#if !(RETAIL)
                _eventBus.Publish(new LegitimacyLockUpEvent());
#endif
                throw;
            }


            Logger.Info($"ResetHandCount:{HandCount} ResidualAmount:{residualAmount}");
        }

        private void SendHandCountChangedEvent()
        {
            _eventBus.Publish(new HandCountChangedEvent(HandCount));
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
    }
}