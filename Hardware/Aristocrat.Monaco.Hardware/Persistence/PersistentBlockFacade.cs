namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System;
    using Contracts.Persistence;
    using Kernel;
    using Newtonsoft.Json;

    /// <summary>
    /// A persistent block facade.
    /// </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Persistence.BlockKeyValueFacade"/>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Persistence.IPersistentBlock"/>
    [Serializable]
    public class PersistentBlockFacade : KeyValueFacade, IPersistentBlock
    {
        private readonly IPersistentStorageManager _persistentStorageManager;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.Persistence.PersistentBlockFacade class.
        /// </summary>
        /// <param name="blockName">Name of the block.</param>
        /// <param name="persistenceLevel">The level.</param>
        [JsonConstructor]
        public PersistentBlockFacade(string blockName, PersistenceLevel persistenceLevel)
            : this(blockName, persistenceLevel,
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the Aristocrat.Monaco.Hardware.Persistence.PersistentBlockFacade class.
        /// </summary>
        /// <param name="blockName">Name of the block.</param>
        /// <param name="level">The level.</param>
        /// <param name="persistentStorageManager">Manager for persistent storage.</param>
        public PersistentBlockFacade(string blockName, PersistenceLevel level,
            IPersistentStorageManager persistentStorageManager)
            : base(blockName)
        {
            BlockName = blockName;
            PersistenceLevel = level;
            _persistentStorageManager = persistentStorageManager ??
                                        throw new ArgumentNullException(nameof(persistentStorageManager));
        }

        /// <inheritdoc/>
        public string BlockName { get; }

        /// <inheritdoc/>
        public PersistenceLevel PersistenceLevel { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public IPersistentTransaction Transaction()
        {
            return new PersistentBlockTransactionFacade(
                BlockName,
                PersistenceLevel,
                _persistentStorageManager);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Aristocrat.Monaco.Hardware.Persistence.PersistentBlockFacade and optionally releases the
        /// managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /// <inheritdoc/>
        protected override bool GetValueInternal<T>(string key, out T value)
        {
            value = default(T);

            if (!_persistentStorageManager.BlockExists(key))
            {
                return false;
            }

            var block = _persistentStorageManager.GetBlock(key);
            var serialized = (string) block[key];
            if (string.IsNullOrEmpty(serialized))
            {
                return false;
            }

            value = JsonConvert.DeserializeObject<T>(serialized);
            return true;
        }

        /// <inheritdoc/>
        protected override bool SetValueInternal<T>(string key, T value)
        {
            var serialized = JsonConvert.SerializeObject(value, Formatting.None);

            IPersistentStorageAccessor block;
            if (!_persistentStorageManager.BlockExists(key))
            {
                var blockFormat = new BlockFormat();
                blockFormat.AddFieldDescription(new FieldDescription(FieldType.UnboundedString, 0, key));

                block = _persistentStorageManager.CreateDynamicBlock(PersistenceLevel, key, 1, blockFormat);
            }
            else
            {
                block = _persistentStorageManager.GetBlock(key);
            }

            block[key] = serialized;
            return true;
        }
    }
}