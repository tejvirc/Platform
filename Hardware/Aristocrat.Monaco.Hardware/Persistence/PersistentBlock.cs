namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System;
    using Contracts.Persistence;

    /// <summary>
    ///     A persistent block.
    /// </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Persistence.IPersistentBlock" />
    public class PersistentBlock : IPersistentBlock
    {
        private IKeyAccessor _accessor;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.Persistence.PersistentBlock class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="level">The level.</param>
        /// <param name="accessor">The accessor.</param>
        public PersistentBlock(string name, PersistenceLevel level, IKeyAccessor accessor)
        {
            BlockName = name;
            PersistenceLevel = level;
            _accessor = accessor;
        }

        /// <inheritdoc />
        public string BlockName { get; }

        /// <inheritdoc />
        public PersistenceLevel PersistenceLevel { get; }

        /// <inheritdoc />
        public bool GetValue<T>(out T value)
        {
            return GetValueInternal(BlockName, out value);
        }

        /// <inheritdoc />
        public bool GetValue<T>(string key, out T value)
        {
            var generatedKey = KeyCreator.BlockFieldKey(BlockName, key);
            return GetValueInternal(generatedKey, out value);
        }

        /// <inheritdoc />
        public bool GetValue<T>(int index, out T value)
        {
            return GetValueInternal(BlockName, index, out value);
        }

        /// <inheritdoc />
        public bool GetValue<T>(string key, int index, out T value)
        {
            var generatedKey = KeyCreator.BlockFieldKey(BlockName, key);
            return GetValueInternal(generatedKey, index, out value);
        }

        /// <inheritdoc />
        public T GetOrCreateValue<T>() where T : new()
        {
            return GetOrCreateValueInternal<T>(BlockName);
        }

        /// <inheritdoc />
        public T GetOrCreateValue<T>(string key) where T : new()
        {
            var generatedKey = KeyCreator.BlockFieldKey(BlockName, key);
            return GetOrCreateValueInternal<T>(generatedKey);
        }

        /// <inheritdoc />
        public bool SetValue<T>(T value)
        {
            return SetValueInternal(BlockName, value);
        }

        /// <inheritdoc />
        public bool SetValue<T>(string key, T value)
        {
            return SetValueInternal(KeyCreator.BlockFieldKey(BlockName, key), value);
        }

        /// <inheritdoc />
        public bool SetValue<T>(int index, T value)
        {
            return SetValueInternal(BlockName, index, value);
        }

        /// <inheritdoc />
        public bool SetValue<T>(string key, int index, T value)
        {
            return SetValueInternal(KeyCreator.BlockFieldKey(BlockName, key), index, value);
        }

        /// <inheritdoc />
        public IPersistentTransaction Transaction()
        {
            IPersistentTransaction transaction =
                ScopedTransactionHolder.ActiveTransaction() ?? new PersistentTransaction(_accessor);
            return new PersistentBlockTransaction(BlockName, transaction);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _accessor = null;
            }
        }

        private bool GetValueInternal<T>(string key, out T value)
        {
            var scoped = ScopedTransactionHolder.ActiveTransaction();

            return scoped != null ? scoped.GetValue(key, out value) : _accessor.GetValue(key, out value);
        }

        private T GetOrCreateValueInternal<T>(string key) where T : new()
        {
            var scoped = ScopedTransactionHolder.ActiveTransaction();

            return scoped != null ? scoped.GetOrCreateValue<T>(key) : _accessor.GetOrCreateValue<T>(key);
        }

        private bool GetValueInternal<T>(string key, int index, out T value)
        {
            var scoped = ScopedTransactionHolder.ActiveTransaction();

            return scoped != null ? scoped.GetValue(key, index, out value) : _accessor.GetValue(key, index, out value);
        }

        private bool SetValueInternal<T>(string key, T value)
        {
            var scoped = ScopedTransactionHolder.ActiveTransaction();

            return scoped?.SetValue(key, value) ?? _accessor.SetValue(key, value);
        }

        private bool SetValueInternal<T>(string key, int index, T value)
        {
            var scoped = ScopedTransactionHolder.ActiveTransaction();
            if (scoped != null)
            {
                return scoped.SetValue(key, index, value);
            }

            return _accessor.SetValue(key, index, value);
        }
    }
}