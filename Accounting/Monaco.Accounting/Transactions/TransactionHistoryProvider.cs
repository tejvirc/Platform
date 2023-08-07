namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the TransactionHistoryProvider class.
    /// </summary>
    public class TransactionHistoryProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly List<ITransaction> _transactions = new List<ITransaction>();
        private readonly object _lockObject = new object();

        private int _currentIndex;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionHistoryProvider" /> class.
        /// </summary>
        /// <param name="transactionType">The type of transaction the provider handles.</param>
        /// <param name="level">The persistence level.</param>
        /// <param name="size">The maximum number of transactions the provider can hold.</param>
        /// <param name="persistable">Indicates if the provider implements IPersistable.</param>
        /// <param name="printable">Indicates if the provider is printable.</param>
        public TransactionHistoryProvider(
            Type transactionType,
            PersistenceLevel level,
            int size,
            bool persistable,
            bool printable)
        {
            TransactionType = transactionType;
            Level = level;
            MaxTransactions = size;
            Persistable = persistable;
            Printable = printable;
            Logger.InfoFormat(
                CultureInfo.CurrentCulture,
                "[CONFIG] Type={0} Level={1} Size={2} Persistable={3} Printable={4}.",
                TransactionType,
                Level,
                MaxTransactions,
                Persistable,
                Printable);
            if (Persistable)
            {
                var serviceManager = ServiceManager.GetInstance();
                var storageService = serviceManager.GetService<IPersistentStorageManager>();
                var largestTransactionId = 0L;
                var lastIndex = -1;
                var blockName = TransactionType.ToString();
                if (storageService.BlockExists(blockName))
                {
                    var block = storageService.GetBlock(blockName);
                    MaxTransactions = Math.Min(MaxTransactions, block.Count);

                    var results = block.GetAll();

                    for (var i = 0; i < MaxTransactions; i++)
                    {
                        var transaction = (ITransaction)Activator.CreateInstance(TransactionType);

                        if (((BaseTransaction)transaction).SetData(results[i]))
                        {
                            _transactions.Add(transaction);
                        }

                        if(transaction.TransactionId > largestTransactionId)
                        {
                            largestTransactionId = transaction.TransactionId;
                            lastIndex = i;
                        }
                    }

                    _currentIndex = (lastIndex + 1) % MaxTransactions;
                }
                else
                {
                    storageService.CreateBlock(Level, blockName, MaxTransactions);
                }
            }
        }

        /// <summary>
        ///     Gets the type of transaction the provider contains.
        /// </summary>
        public Type TransactionType { get; }

        /// <summary>
        ///     Gets the total number of entries before queue-cycling.
        /// </summary>
        public int MaxTransactions { get; }

        /// <summary>
        ///     Gets the persistence level of the TransactionHistoryProvider.
        /// </summary>
        public PersistenceLevel Level { get; }

        /// <summary>
        ///     Gets a value indicating whether the transaction implements IPersistable.
        /// </summary>
        public bool Persistable { get; }

        /// <summary>
        ///     Gets a value indicating whether the provider is printable.
        /// </summary>
        public bool Printable { get; }

        /// <summary>
        ///     Method to recall all transactions of a specified transaction type from the TransactionProvider.
        /// </summary>
        /// <typeparam name="T">The <see cref="ITransaction"/> type to return</typeparam>
        /// <returns>Collection of the transactions stored for the specified type.</returns>
        public IReadOnlyCollection<T> RecallTransactions<T>() where T : ITransaction
        {
            lock (_lockObject)
            {
                Logger.Debug($"Recalling {_transactions.Count} transactions...");
                return _transactions.Select(transaction => transaction.Clone() as ITransaction).OfType<T>().ToList();
            }
        }

        /// <summary>
        ///     Method to recall all transactions of a specified transaction type from the TransactionProvider.
        /// </summary>
        /// <returns>Collection of the transactions stored for the specified type.</returns>
        public IReadOnlyCollection<ITransaction> RecallTransactions()
        {
            lock (_lockObject)
            {
                Logger.Debug($"Recalling {_transactions.Count} transactions...");
                return _transactions.Select(transaction => transaction.Clone() as ITransaction).ToList();
            }
        }

        /// <summary>
        ///     Method to update a transaction to the TransactionProvider.
        /// </summary>
        /// <param name="transaction">Transaction to be updated.</param>
        public void UpdateTransaction(ITransaction transaction)
        {
            UpdateTransaction(transaction, 0);
        }

        /// <summary>
        ///     Method to update a transaction to the TransactionProvider.
        /// </summary>
        /// <param name="transaction">Transaction to be updated.</param>
        /// <param name="oldTransactionId">Transaction Id to overwrite</param>
        public void UpdateTransaction(ITransaction transaction, long oldTransactionId)
        {
            if (oldTransactionId == 0)
            {
                oldTransactionId = transaction.TransactionId;
            }

            Logger.Debug($"Updating transaction:\n{transaction}");

            int element;
            lock (_lockObject)
            {
                element = _transactions.FindIndex(a => a.TransactionId == oldTransactionId);

                _transactions[element] = transaction;
            }

            if (Persistable)
            {
                Logger.Debug("Updating transaction to PersistentStorage...");

                var storageService = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
                var block = storageService.GetBlock(transaction.GetType().ToString());

                ((BaseTransaction)transaction).SetPersistence(block, element);
            }
        }

        /// <summary>
        ///     Method to save a transaction to the TransactionProvider.
        /// </summary>
        /// <param name="transaction">Transaction to be saved.</param>
        public void SaveTransaction(ITransaction transaction)
        {
            Logger.Debug($"Saving transaction:\n{transaction}");

            lock (_lockObject)
            {
                if (_transactions.Count < MaxTransactions)
                {
                    _transactions.Add(transaction);
                }
                else
                {
                    _transactions[_currentIndex] = transaction;
                }
            }

            if (Persistable)
            {
                Logger.Debug("Writing transaction to PersistentStorage...");

                var storageService = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
                var block = storageService.GetBlock(transaction.GetType().ToString());

                ((BaseTransaction)transaction).SetPersistence(block, _currentIndex);
                _currentIndex = (_currentIndex + 1) % MaxTransactions;
            }
        }

        /// <summary>
        ///     Returns a human-readable represenation of the TransactionHistoryProvider class.
        /// </summary>
        /// <returns>A human-readable string.</returns>
        public override string ToString()
        {
            var message = new StringBuilder();

            message.Append("This TransactionHistoryProvider has the following values:\n");
            message.AppendFormat("TransactionType: {0}\n", TransactionType);
            message.AppendFormat("MaxTransactions: {0}\n", MaxTransactions);
            message.AppendFormat("PersistenceLevel: {0}\n", Level);
            message.AppendFormat("Persistable: {0}\n", Persistable);
            message.AppendFormat("Printable: {0}\n", Printable);
            message.AppendFormat("It currently contains {0} transactions", _transactions.Count);

            return message.ToString();
        }
    }
}
