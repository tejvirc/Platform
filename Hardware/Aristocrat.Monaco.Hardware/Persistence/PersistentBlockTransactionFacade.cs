namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Aristocrat.Monaco.Common;
    using Contracts.Persistence;
    using log4net;
    using Newtonsoft.Json;

    /// <summary>
    ///     A persistent transaction facade.
    /// </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Persistence.IPersistentTransaction"/>
    public class PersistentBlockTransactionFacade : KeyValueFacade, IPersistentTransaction
    {
        private readonly PersistenceLevel _level;
        private readonly Dictionary<string, string> _updates = new Dictionary<string, string>();
        private readonly IPersistentStorageManager _persistentStorageManager;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.Persistence.PersistentTransactionFacade class.
        /// </summary>
        /// <param name="blockName">Name of the block.</param>
        /// <param name="level">The level.</param>
        /// <param name="persistentStorageManager">Manager for persistent storage.</param>
        public PersistentBlockTransactionFacade(string blockName, PersistenceLevel level,
            IPersistentStorageManager persistentStorageManager)
            : base(blockName)
        {
            _level = level;
            _persistentStorageManager = persistentStorageManager;
        }

        /// <inheritdoc />
        public event EventHandler<TransactionEventArgs> Completed;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool Commit()
        {
            IScopedTransaction createdScope = null;
            var transaction = PersistenceTransaction.Current;
            if (transaction == null)
            {
                createdScope = _persistentStorageManager.ScopedTransaction();
                transaction = PersistenceTransaction.Current;
            }

            foreach (var update in _updates)
            {
                var accessor = _persistentStorageManager.GetBlock(update.Key);
                accessor[update.Key] = update.Value;
            }

            if (createdScope == null)
            {
                return true;
            }

            transaction.OnCompleted += OnScopeCompleted;
            createdScope.Complete();
            transaction.OnCompleted -= OnScopeCompleted;
            createdScope.Dispose();

            return true;
        }

        /// <inheritdoc/>
        protected override bool GetValueInternal<T>(string key, out T value)
        {
            value = default(T);

            if (!_persistentStorageManager.BlockExists(key))
            {
                return false;
            }

            string serialized;
            if (_updates.TryGetValue(key, out var result))
            {
                serialized = result;
            }
            else
            {
                var block = _persistentStorageManager.GetBlock(key);
                serialized = (string)block[key];
                if (string.IsNullOrEmpty(serialized))
                {
                    return false;
                }
            }

            value = JsonConvert.DeserializeObject<T>(serialized);

            _updates[key] = serialized;
            return true;
        }

        /// <inheritdoc/>
        protected override bool SetValueInternal<T>(string key, T value)
        {
            string serialized;
            using (var _ = new Common.ScopedMethodTimer(
                       Logger.DebugMethodLogger,
                       Logger.DebugMethodTraceLogger,
                       $"SetValueInternal type:",
                       "JsonConvert_Serialize",
                       "Done"))
            {
                serialized = JsonConvert.SerializeObject(value, Formatting.None);
            }

            if (!_persistentStorageManager.BlockExists(key))
            {
                var blockFormat = new BlockFormat();
                blockFormat.AddFieldDescription(new FieldDescription(FieldType.UnboundedString, 0, key));
                _persistentStorageManager.CreateDynamicBlock(_level, key, 1, blockFormat);
            }

            _updates[key] = serialized;
            return true;
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the Aristocrat.Monaco.Hardware.Persistence.PersistentBlockFacade 
        ///     and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updates.Clear();
            }
        }

        /// <summary>
        ///     Raises the transaction event.
        /// </summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnCompleted(TransactionEventArgs e)
        {
            Completed?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the transaction event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnScopeCompleted(object sender, TransactionEventArgs e)
        {
            OnCompleted(e);
        }
    }
}