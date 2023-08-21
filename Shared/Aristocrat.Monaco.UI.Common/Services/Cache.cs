
namespace Aristocrat.Monaco.UI.Common.Services
{
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Caching;

    /// <summary>
    /// Cache allows for storing of objects with a time based expiration plus the ability to flush the data when specific events happen
    /// </summary>
    public class Cache : ICache, IService, IDisposable
    {
        private MemoryCache _cache;
        private readonly IEventBus _eventBus;
        private bool _disposed;
        private const string CacheName = "MonacoCache";
        private readonly Dictionary<string, List<Type>> _cacheMap = new Dictionary<string, List<Type>>();
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object _lock = new object();
        private readonly TimeSpan _maxCacheTimeout = TimeSpan.FromMinutes(120);
        private readonly TimeSpan _defaultCacheTimeout = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Constructor
        /// </summary>
        public Cache()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventBus"></param>
        public Cache(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public string Name => "Cache Service";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ICache) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Debug(Name + " OnInitialize()");
            _cache = new MemoryCache(CacheName);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool Add<T>(string key, T value, List<Type> invalidateEvents, TimeSpan timeout) where T : class
        {
            var actualTimeout = timeout > _maxCacheTimeout ? _maxCacheTimeout : timeout;
            bool success = false;
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(actualTimeout)
            };
            lock (_lock)
            {
                if (_cache.Add(key, value, policy))
                {
                    foreach (var eventType in invalidateEvents)
                    {
                        if (GetEventCount(eventType) == 0)
                        {
                            Subscribe(eventType);
                        }
                    }

                    _cacheMap[key] = invalidateEvents;
                    success = true;
                }
            }

            return success;
        }

        /// <inheritdoc />
        public bool Add<T>(string key, T value, TimeSpan timeout) where T : class
        {
            return Add(key, value, null, timeout);
        }

        /// <inheritdoc />
        public bool Add<T>(string key, T value, List<Type> invalidateEvents) where T : class
        {
            return Add(key, value, invalidateEvents, _defaultCacheTimeout);
        }

        /// <inheritdoc />
        public bool Add<T>(string key, T value) where T : class
        {
            return Add(key, value, null, _defaultCacheTimeout);
        }

        /// <inheritdoc />
        public T AddOrGetExisting<T>(string key, T value) where T : class
        {
            lock (_lock)
            {
                var item = Get<T>(key);
                if (item == null)
                {
                    Add(key, value);
                    item = value;
                }
                return item;
            }
        }

        /// <inheritdoc />
        public T Get<T>(string key) where T : class
        {
            lock (_lock)
            {
                return _cache.Get(key) as T;
            }
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            lock (_lock)
            {
                _cache.Remove(key);
                ClearKeyFromMap(key);
            }
        }

        private void ClearKeyFromMap(string key)
        {
            if (_cacheMap.ContainsKey(key))
            {
                var eventList = _cacheMap[key];
                _cacheMap.Remove(key);
                foreach (var eventType in eventList)
                {
                    if (GetEventCount(eventType) == 0)
                    {
                        Unsubscribe(eventType);
                    }
                }
            }
        }

        private void Subscribe(Type type)
        {
            _eventBus.Subscribe(this, type, OnEvent);
        }

        private void Unsubscribe(Type type)
        {
            _eventBus.Unsubscribe(this, type);
        }

        private void OnEvent(IEvent theEvent)
        {
            lock (_lock)
            {
                var type = theEvent.GetType();
                var cacheItems = GetCacheItemsForEvent(type);

                foreach (var item in cacheItems)
                {
                    Remove(item);
                }
            }
        }

        private int GetEventCount(Type eventType)
        {
            var events = _cacheMap.SelectMany(o => o.Value);
            return events.Count(o => o == eventType);
        }

        private List<string> GetCacheItemsForEvent(Type eventType)
        {
            return _cacheMap.Where(o => o.Value.Contains(eventType)).Select(o => o.Key).ToList();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
                _cache.Dispose();
                _cache = null;
            }

            _disposed = true;
        }
    }
}

