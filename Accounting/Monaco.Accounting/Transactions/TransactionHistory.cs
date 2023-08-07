namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Contracts;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the TransactionHistory class.
    /// </summary>
    public class TransactionHistory : ITransactionHistory, IService
    {
        private const string TransactionsExtensionPath = "/Accounting/TransactionHistories";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly Dictionary<Type, TransactionHistoryProvider> _transactionProviders =
            new Dictionary<Type, TransactionHistoryProvider>();

        /// <inheritdoc />
        public string Name => typeof(TransactionHistory).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ITransactionHistory) };

        /// <inheritdoc />
        public void Initialize()
        {
            CreateTransactionProviders();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Type> TransactionTypes => new HashSet<Type>(_transactionProviders.Keys);

        /// <inheritdoc />
        public IReadOnlyCollection<T> RecallTransactions<T>() where T : ITransaction
        {
            Logger.Debug($"Attempting to recall transactions of type: {typeof(T)}");

            return GetTransactionHistoryProvider<T>().RecallTransactions<T>();
        }

        /// <inheritdoc />
        public IOrderedEnumerable<ITransaction> RecallTransactions()
        {
            return RecallTransactions(true);
        }

        /// <inheritdoc />
        public IOrderedEnumerable<ITransaction> RecallTransactions(bool newestFirst)
        {
            var transactions = new List<ITransaction>();
            foreach (var provider in _transactionProviders.Values)
            {
                transactions.AddRange(provider.RecallTransactions());
            }

            return newestFirst
                ? transactions.OrderByDescending(t => t.TransactionId)
                : transactions.OrderBy(t => t.TransactionId);
        }

        /// <inheritdoc />
        public int GetMaxTransactions<T>() where T : ITransaction
        {
            return GetTransactionHistoryProvider<T>().MaxTransactions;
        }

        /// <inheritdoc />
        public bool IsPrintable<T>() where T : ITransaction
        {
            return GetTransactionHistoryProvider<T>().Printable;
        }

        /// <inheritdoc />
        public void AddTransaction(ITransaction transaction)
        {
            Logger.Debug($"Attempting to save transaction:\n{transaction}");

            if (transaction.TransactionId != 0)
            {
                throw new NotSupportedException("Transaction id is not 0");
            }

            var idProvider = ServiceManager.GetInstance().GetService<IIdProvider>();

            transaction.TransactionId = idProvider.GetNextTransactionId();

            if (transaction.LogSequence == 0)
            {
                transaction.LogSequence = idProvider.GetNextLogSequence(transaction.GetType());
            }

            GetTransactionHistoryProvider(transaction.GetType()).SaveTransaction(transaction);

            Logger.Debug($"Transaction type for {transaction.TransactionId} saved to history...");

            ServiceManager.GetInstance().GetService<IEventBus>().Publish(new TransactionSavedEvent(transaction));
        }

        /// <inheritdoc />
        public void UpdateTransaction(ITransaction transaction)
        {
            Logger.Debug($"Attempting to update transaction:\n{transaction}");

            if (transaction.TransactionId == 0)
            {
                throw new NotSupportedException("Transaction id is 0");
            }

            GetTransactionHistoryProvider(transaction.GetType()).UpdateTransaction(transaction);
        }

        /// <inheritdoc />
        public void OverwriteTransaction(long transactionId, ITransaction transaction)
        {
            Logger.Debug($"Attempting to overwrite transaction:\n{transactionId} with {transaction}");

            if (transaction.LogSequence == 0)
            {
                throw new NotSupportedException("LogSequence is 0");
            }

            if (transaction.TransactionId == 0)
            {
                transaction.TransactionId =
                    ServiceManager.GetInstance().GetService<IIdProvider>().GetNextTransactionId();
            }

            GetTransactionHistoryProvider(transaction.GetType()).UpdateTransaction(transaction, transactionId);
        }

        private static Exception CreateTransactionHistoryException(string message, Exception insideException, ITransaction transaction)
        {
            Logger.Error(message);

            var exception = new TransactionHistoryException(message, insideException);

            if (transaction != null)
            {
                exception.AttachTransaction(transaction);
            }

            return exception;
        }

        private void CreateTransactionProviders()
        {
            Logger.Debug("Create TransactionProviders...");

            var nodes =
                MonoAddinsHelper.GetSelectedNodes<TransactionHistoryProviderExtensionNode>(TransactionsExtensionPath);
            foreach (var node in nodes)
            {
                var persisted = node.Type.IsSubclassOf(typeof(BaseTransaction));

                _transactionProviders.Add(
                    node.Type,
                    new TransactionHistoryProvider(
                        node.Type,
                        node.Level,
                        node.MaxTransactions,
                        persisted,
                        node.IsPrintable));
            }
        }

        private TransactionHistoryProvider GetTransactionHistoryProvider(Type type)
        {
            if (!_transactionProviders.TryGetValue(type, out var result))
            {
                throw CreateTransactionHistoryException(
                    $"No matching provider found for current transaction of Type: {type}",
                    null,
                    null);
            }

            return result;
        }

        private TransactionHistoryProvider GetTransactionHistoryProvider<T>() where T : ITransaction
        {
            return GetTransactionHistoryProvider(typeof(T));
        }
    }
}
