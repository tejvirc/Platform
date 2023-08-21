namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts.Persistence;
    using Kernel;
    using Persistence;

    /// <summary> A persistence provider. </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Persistence.IPersistenceProvider"/>
    public class PersistenceProvider : IPersistenceProvider, IService
    {
        private KeyAccessor _accessor;

        private readonly ConcurrentDictionary<string, IPersistentBlock> _persistentBlocks =
            new ConcurrentDictionary<string, IPersistentBlock>();

        /// <inheritdoc/>
        public string Name => GetType().ToString();

        /// <inheritdoc/>
        public ICollection<Type> ServiceTypes => new[] { typeof(IPersistenceProvider) };

        /// <inheritdoc/>
        public void Initialize()
        {
            _accessor = new KeyAccessor(new SqlitePersistentStore());
            var block = GetOrCreateBlock(KeyConstants.LevelStaticClearedKey, PersistenceLevel.Static);
            block.SetValue(new LevelClearedInfo
            {
                LastClearTime = DateTime.Now
            });

            block = GetOrCreateBlock(KeyConstants.LevelCriticalClearedKey, PersistenceLevel.Critical);
            block.SetValue(new LevelClearedInfo
            {
                LastClearTime = DateTime.Now,
                JustExecuted = true
            });

            block = GetOrCreateBlock(KeyConstants.LevelTransientClearedKey, PersistenceLevel.Transient);
            block.SetValue(new LevelClearedInfo
            {
                LastClearTime = DateTime.Now
            });

            var eventBus = ServiceManager.GetInstance().TryGetService<IEventBus>();
            eventBus.Subscribe<PersistentStorageClearReadyEvent>(this, HandleEvent); 
        }

        /// <inheritdoc/>
        public IPersistentBlock GetBlock(string key)
        {
            if (_persistentBlocks.TryGetValue(key, out var block))
            {
                return block;
            }

            if (!_accessor.GetPersistenceLevel(key, out var level))
            {
                return null;
            }

            // block persistence level info exists in the persistent storage
            // but doesn't exist in the cache. Creating and adding to the cache here.
            block = new PersistentBlock(key, level, _accessor);
            return _persistentBlocks.TryAdd(key, block) ? block : null;
        }

        /// <inheritdoc/>
        public IPersistentBlock GetOrCreateBlock(string key, PersistenceLevel level)
        {
            var block = GetBlock(key);
            if (block != null)
            {
                return block;
            }

            if (!_accessor.SetPersistenceLevel(key, level))
            {
                return null;
            }

            block = new PersistentBlock(key, level, _accessor);
            return _persistentBlocks.TryAdd(key, block) ? block : null;
        }

        /// <inheritdoc/>
        public IScopedTransaction ScopedTransaction()
        {
            return ScopedTransactionHolder.CreateTransaction(_accessor);
        }

        /// <inheritdoc/>
        public async Task Verify(bool full)
        {
            await Task.Run(() =>
            {
                if (!_accessor.Verify(full))
                {
                    PostEvent(new PersistentStorageIntegrityCheckFailedEvent());
                }
            });
        }

        protected virtual void HandleEvent(PersistentStorageClearReadyEvent @event)
        {
            PostEvent(new PersistentStorageClearStartedEvent(@event.Level));
            if (_accessor.Clear(@event.Level))
            {
                var key = LevelClearedKey(@event.Level);
                var block = GetBlock(key);
                block.SetValue(
                    key,
                    new LevelClearedInfo
                    {
                        LastClearTime = DateTime.Now,
                        JustExecuted = true
                    });

                PostEvent(new PersistentStorageClearedEvent(@event.Level));
            }
        }

        private void PostEvent<T>(T @event) where T : IEvent
        {
            ServiceManager.GetInstance().TryGetService<IEventBus>()?.Publish(@event);
        }

        private string LevelClearedKey(PersistenceLevel level)
        {
            switch (level)
            {
                case PersistenceLevel.Critical:
                    return KeyConstants.LevelCriticalClearedKey;
                case PersistenceLevel.Static:
                    return KeyConstants.LevelStaticClearedKey;
                case PersistenceLevel.Transient:
                    return KeyConstants.LevelTransientClearedKey;
                default:
                    return string.Empty;
            } 
        }
    }
}