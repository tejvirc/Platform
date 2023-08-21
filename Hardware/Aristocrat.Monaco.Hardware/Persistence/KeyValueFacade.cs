namespace Aristocrat.Monaco.Hardware.Persistence
{
    using Contracts.Persistence;

    /// <summary>
    ///     A key value facade.
    /// </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Persistence.IKeyValueAccessor" />
    public abstract class KeyValueFacade : IKeyValueAccessor
    {
        private readonly string _blockName;

        protected KeyValueFacade(string block)
        {
            _blockName = block;
        }

        /// <inheritdoc />
        public bool GetValue<T>(out T value)
        {
            return GetValueInternal(KeyCreator.Key(_blockName, typeof(T)), out value);
        }

        /// <inheritdoc />
        public bool GetValue<T>(string key, out T value)
        {
            return GetValueInternal(KeyCreator.Key(KeyCreator.BlockFieldKey(_blockName, key), typeof(T)), out value);
        }

        /// <inheritdoc />
        public bool GetValue<T>(int index, out T value)
        {
            return GetValueInternal(KeyCreator.IndexedKey(_blockName, index, typeof(T)), out value);
        }

        /// <inheritdoc />
        public bool GetValue<T>(string key, int index, out T value)
        {
            return GetValueInternal(KeyCreator.IndexedKey(KeyCreator.BlockFieldKey(_blockName, key), index, typeof(T)), out value);
        }

        /// <inheritdoc />
        public T GetOrCreateValue<T>() where T : new()
        {
            if (GetValue(out T result))
            {
                return result;
            }

            var value = new T();
            SetValue(value);
            return value;
        }

        /// <inheritdoc />
        public T GetOrCreateValue<T>(string key) where T : new()
        {
            if (GetValue(key, out T result))
            {
                return result;
            }

            var value = new T();
            SetValue(key, value);
            return value;
        }

        /// <inheritdoc />
        public bool SetValue<T>(T value)
        {
            return SetValueInternal(KeyCreator.Key(_blockName, typeof(T)), value);
        }

        /// <inheritdoc />
        public bool SetValue<T>(string key, T value)
        {
            return SetValueInternal(KeyCreator.Key(KeyCreator.BlockFieldKey(_blockName, key), typeof(T)), value);
        }

        /// <inheritdoc />
        public bool SetValue<T>(int index, T value)
        {
            return SetValueInternal(KeyCreator.IndexedKey(_blockName, index, typeof(T)), value);
        }

        /// <inheritdoc />
        public bool SetValue<T>(string key, int index, T value)
        {
            return SetValueInternal(
                KeyCreator.IndexedKey(KeyCreator.BlockFieldKey(_blockName, key), index, typeof(T)),
                value);
        }

        /// <summary>
        ///     Gets value internal.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">[out] The value.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        protected abstract bool GetValueInternal<T>(string key, out T value);

        /// <summary>
        ///     Sets value internal.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">[out] The value.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        protected abstract bool SetValueInternal<T>(string key, T value);
    }
}