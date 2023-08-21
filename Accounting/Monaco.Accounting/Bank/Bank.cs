namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using log4net;

    /// <summary>
    ///     Definition of the Bank class.
    /// </summary>
    public sealed class Bank : IService, IBank, IDisposable
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<AccountType, long> _accounts = new Dictionary<AccountType, long>();
        private readonly IEventBus _bus;
        private readonly INoteAcceptorMonitor _noteAcceptorMonitor;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionCoordinator _transactionCoordinator;

        private ReaderWriterLockSlim _accountsLock = new ReaderWriterLockSlim();
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Bank" /> class.
        /// </summary>
        public Bank()
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ITransactionCoordinator>(),
                ServiceManager.GetInstance().TryGetService<INoteAcceptorMonitor>())
        {
        }

        [CLSCompliant(false)]
        public Bank(
            IPropertiesManager properties,
            IPersistentStorageManager storage,
            IEventBus bus,
            ITransactionCoordinator transactionCoordinator,
            INoteAcceptorMonitor noteAcceptorMonitor)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            _noteAcceptorMonitor = noteAcceptorMonitor;

            _accountsLock.EnterWriteLock();
            try
            {
                // Populate the _accounts dictionary with all available AccountTypes
                foreach (int item in Enum.GetValues(typeof(AccountType)))
                {
                    _accounts.Add((AccountType)item, 0);
                }
            }
            finally
            {
                _accountsLock.ExitWriteLock();
            }

            var blockName = GetType().ToString();
            if (_storage.BlockExists(blockName))
            {
                ReceivePersistence(_storage.GetBlock(blockName));
            }
            else
            {
                _storage.CreateBlock(Level, blockName, 1);
            }

            var balance = QueryBalance();

            _properties.SetProperty(PropertyKey.CurrentBalance, balance);

            _noteAcceptorMonitor?.SetCurrentCredits(balance);
        }

        /// <inheritdoc />
        public long Limit => _properties.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue);

        /// <inheritdoc />
        public bool CheckDeposit(AccountType account, long amount, Guid transactionId)
        {
            // NOTE: If we enforce a limit check here we will prevent wins from being deposited. Something else needs to check the balance against the limit and perform the appropriate action

            // Check if we are currently using the correct Guid
            if (!_transactionCoordinator.VerifyCurrentTransaction(transactionId))
            {
                throw CreateBankException($"Incorrect Transaction Guid: {transactionId}", null);
            }

            _accountsLock.EnterReadLock();
            try
            {
                if (!_accounts.ContainsKey(account))
                {
                    throw CreateBankException($"Account [{account}] not found in the Bank", null);
                }

                return true;
            }
            finally
            {
                _accountsLock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public bool CheckWithdraw(AccountType account, long amount, Guid transactionId)
        {
            if (transactionId == Guid.Empty)
            {
                Logger.Warn($"CheckWithdrawal transactionId {transactionId}");
                return false;
            }

            // Is the current transaction active?
            if (!_transactionCoordinator.IsTransactionActive)
            {
                Logger.Warn($"CheckWithdrawal transactionId {transactionId} with current transaction not active.");
                return false;
            }

            // Is the current transaction for this transaction ID?
            if (!_transactionCoordinator.VerifyCurrentTransaction(transactionId))
            {
                throw CreateBankException($"Incorrect Transaction Guid: {transactionId}", null);
            }

            if (QueryBalance(account) - amount >= 0)
            {
                return true;
            }

            Logger.Warn($"Cannot withdraw amount: {amount}, exceeds balance");

            return false;
        }

        /// <inheritdoc />
        public void Deposit(AccountType account, long amount, Guid transactionId)
        {
            if (amount == 0)
            {
                return;
            }

            if (CheckDeposit(account, amount, transactionId))
            {
                var oldBalance = QueryBalance();

                Logger.Debug(
                    $"Current balance={oldBalance} Depositing={amount} Updating={account} New Balance={QueryBalance(account) + amount}");

                _accountsLock.EnterWriteLock();
                try
                {
                    _accounts[account] += amount;
                }
                finally
                {
                    _accountsLock.ExitWriteLock();
                }

                Save(oldBalance, QueryBalance(), transactionId);
            }
        }

        /// <inheritdoc />
        public void Withdraw(AccountType account, long amount, Guid transactionId)
        {
            if (amount == 0)
            {
                return;
            }

            var oldBalance = QueryBalance();

            if (CheckWithdraw(account, amount, transactionId))
            {
                Logger.Debug(
                    $"Current balance={oldBalance} Withdrawing={amount} Updating={account} New Balance={QueryBalance(account) - amount}");

                _accountsLock.EnterWriteLock();
                try
                {
                    _accounts[account] -= amount;
                }
                finally
                {
                    _accountsLock.ExitWriteLock();
                }

                Save(oldBalance, QueryBalance(), transactionId);
            }
            else
            {
                throw CreateBankException(
                    $"Unable to withdraw {amount} from the account {account} with balance of {oldBalance}",
                    null);
            }
        }

        /// <inheritdoc />
        public long QueryBalance()
        {
            _accountsLock.EnterReadLock();
            try
            {
                return _accounts.Sum(item => item.Value);
            }
            finally
            {
                _accountsLock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public long QueryBalance(AccountType account)
        {
            _accountsLock.EnterReadLock();
            try
            {
                if (!_accounts.TryGetValue(account, out var ret))
                {
                    throw CreateBankException($"Account [{account}] not found in the Bank", null);
                }

                return ret;
            }
            finally
            {
                _accountsLock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_accountsLock != null)
            {
                _accountsLock.Dispose();
                _accountsLock = null;
            }

            _disposed = true;
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IBank) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private static Exception CreateBankException(string message, Exception insideException)
        {
            Logger.Fatal(message);

            return new BankException(message, insideException);
        }

        private void ReceivePersistence(IPersistentStorageAccessor block)
        {
            _accountsLock.EnterWriteLock();
            try
            {
                foreach (int item in Enum.GetValues(typeof(AccountType)))
                {
                    _accounts[(AccountType)item] = (long)block[((AccountType)item).ToString()];
                }
            }
            finally
            {
                _accountsLock.ExitWriteLock();
            }
        }

        private void Save(long oldBalance, long newBalance, Guid transactionId)
        {
            var block = _storage.GetBlock(GetType().ToString());

            _accountsLock.EnterReadLock();
            try
            {
                using (var transaction = block.StartTransaction())
                {
                    foreach (int item in Enum.GetValues(typeof(AccountType)))
                    {
                        transaction[((AccountType)item).ToString()] = _accounts[(AccountType)item];
                    }

                    transaction.Commit();
                }
            }
            finally
            {
                _accountsLock.ExitReadLock();
            }

            _properties.SetProperty(PropertyKey.CurrentBalance, newBalance);

            _bus.Publish(new BankBalanceChangedEvent(oldBalance, newBalance, transactionId));

            _noteAcceptorMonitor?.SetCurrentCredits(newBalance);
        }
    }
}