﻿namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts.Persistence;
    using Kernel;
    using Newtonsoft.Json;
    using Persistence;

    /// <summary>
    ///     A persistence provider facade.
    /// </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Persistence.IPersistenceProvider" />
    public class PersistenceProviderFacade : IPersistenceProvider, IService
    {
        private const string ProviderBlockKey = @"PersistenceProvider";
        private const string ProviderBlockFieldKey = @"PersistentBlocks";

        private ConcurrentDictionary<string, PersistentBlockFacade> _persistentBlocks =
            new ConcurrentDictionary<string, PersistentBlockFacade>();

        private IPersistentStorageManager _persistentStorageManager;

        /// <inheritdoc />
        public IPersistentBlock GetBlock(string key)
        {
            return !_persistentBlocks.TryGetValue(key, out var block) ? null : block;
        }

        /// <inheritdoc />
        public IPersistentBlock GetOrCreateBlock(string key, PersistenceLevel level)
        {
            var block = GetBlock(key);
            if (block != null)
            {
                return block;
            }

            var newBlock = new PersistentBlockFacade(key, level, _persistentStorageManager);
            if (!_persistentBlocks.TryAdd(key, newBlock))
            {
                return null;
            }

            Persist();
            return newBlock;
        }

        /// <inheritdoc />
        public IScopedTransaction ScopedTransaction()
        {
            return _persistentStorageManager.ScopedTransaction();
        }

        /// <inheritdoc />
        public Task Verify(bool full)
        {
            return Task.Run(
                () => { _persistentStorageManager.VerifyIntegrity(full); });
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPersistenceProvider) };

        /// <inheritdoc />
        public void Initialize()
        {
            _persistentStorageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            if (_persistentStorageManager == null)
            {
                throw new ServiceNotFoundException(nameof(_persistentStorageManager));
            }

            var providerBlock = GetProviderBlock();
            if (providerBlock == null)
            {
                throw new NullReferenceException(nameof(providerBlock));
            }

            var serialized = (string)providerBlock[ProviderBlockFieldKey];
            if (string.IsNullOrEmpty(serialized))
            {
                return;
            }

            _persistentBlocks =
                JsonConvert.DeserializeObject<ConcurrentDictionary<string, PersistentBlockFacade>>(serialized);
        }

        private IPersistentStorageAccessor GetProviderBlock()
        {
            if (_persistentStorageManager.BlockExists(ProviderBlockKey))
            {
                var block = _persistentStorageManager.GetBlock(ProviderBlockKey);
                if (block != null)
                {
                    return block;
                }
            }

            var blockFormat = new BlockFormat();
            blockFormat.AddFieldDescription(new FieldDescription(FieldType.UnboundedString, 0, ProviderBlockFieldKey));
            return _persistentStorageManager.CreateDynamicBlock(
                PersistenceLevel.Critical,
                ProviderBlockKey,
                1,
                blockFormat);
        }

        private void Persist()
        {
            var block = GetProviderBlock();
            if (block == null)
            {
                return;
            }

            var serialized = JsonConvert.SerializeObject(_persistentBlocks, Formatting.None);
            block[ProviderBlockFieldKey] = serialized;
        }
    }
}