namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System;
    using System.Collections.Generic;
    using Contracts.Persistence;

    /// <summary> A transaction. </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Persistence.IPersistentTransaction"/>
    /// <seealso cref="T:ITransaction"/>
    public class PersistentTransaction : IPersistentTransaction
    {
        protected IKeyAccessor Accessor;
        protected Dictionary<string, object> Updates = new Dictionary<string, object>();
        protected Dictionary<string, Dictionary<int, object>> IndexedUpdates = new Dictionary<string, Dictionary<int, object>>();

        /// <inheritdoc/>
        public event EventHandler<TransactionEventArgs> Completed;

        public PersistentTransaction(IKeyAccessor accessor)
        {
            Accessor = accessor;
        }

        /// <inheritdoc/>
        public virtual bool GetValue<T>(string key, out T value)
        {
            if (!Updates.TryGetValue(key, out var result))
            {
                var exists = Accessor.GetValue(key, out value);
                if (exists)
                {
                    Updates.Add(key, value);
                }

                return exists;
            }

            value = (T)result;
            return true;
        }

        /// <inheritdoc/>
        public bool GetValue<T>(out T value)
        {
            return GetValue(KeyCreator.Key(typeof(T)), out value);
        }

        /// <inheritdoc/>
        public virtual bool GetValue<T>(string key, int index, out T value)
        {
            if (!IndexedUpdates.TryGetValue(key, out var indexer))
            {
                var exists = Accessor.GetValue(key, index, out value);
                if (exists)
                {
                    IndexedUpdates.Add(key, new Dictionary<int, object> { [index] = value });
                }

                return exists;
            }

            if (!indexer.TryGetValue(index, out var result))
            {
                var exists = Accessor.GetValue(key, index, out value);
                if (exists)
                {
                    indexer.Add(index, value);
                }

                return exists;
            }

            value = (T)result;
            return true;
        }

        /// <inheritdoc/>
        public bool GetValue<T>(int index, out T value)
        {
            return GetValue(KeyCreator.Key(typeof(T)), index, out value);
        }

        /// <inheritdoc/>
        public virtual T GetOrCreateValue<T>(string key) where T : new()
        {
            if (!Updates.TryGetValue(key, out var result))
            {
                return Accessor.GetOrCreateValue<T>(key);
            }

            return (T)result;
        }

        /// <inheritdoc/>
        public T GetOrCreateValue<T>() where T : new()
        {
            return GetOrCreateValue<T>(KeyCreator.Key(typeof(T)));
        }

        /// <inheritdoc/>
        public virtual bool SetValue<T>(string key, T value)
        {
            Updates[key] = value;
            return true;
        }

        /// <inheritdoc/>
        public bool SetValue<T>(T value)
        {
            return SetValue(KeyCreator.Key(typeof(T)), value);
        }

        /// <inheritdoc/>
        public virtual bool SetValue<T>(string key, int index, T value)
        {
            if (!IndexedUpdates.TryGetValue(key, out var indexer))
            {
                IndexedUpdates.Add(key, new Dictionary<int, object> { [index] = value });
                return true;
            }

            indexer[index] = value;
            return true;
        }

        /// <inheritdoc/>
        public bool SetValue<T>(int index, T value)
        {
            return SetValue(KeyCreator.Key(typeof(T)), index, value);
        }

        /// <inheritdoc/>
        public virtual bool Commit()
        {
            var updates = new List<((string prefix, int? index) key, object value)>();
            foreach (var item in Updates)
            {
                updates.Add(((item.Key, null), item.Value));
            }

            foreach (var indexer in IndexedUpdates)
            {
                foreach (var item in indexer.Value)
                {
                    updates.Add(((indexer.Key, item.Key), item.Value));
                }
            }

            var status = Accessor.Commit(updates);
            OnCompleted(new TransactionEventArgs(status));
            return status;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Updates.Clear();
                Updates = null;

                IndexedUpdates.Clear();
                IndexedUpdates = null;

                Accessor = null;
            }
        }

        protected virtual void OnCompleted(TransactionEventArgs e)
        {
            Completed?.Invoke(this, e);
        }
    }
}