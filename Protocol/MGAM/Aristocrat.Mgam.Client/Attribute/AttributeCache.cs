namespace Aristocrat.Mgam.Client.Attribute
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    /// <summary>
    ///     Stores server attributes on the VLT.
    /// </summary>
    internal sealed class AttributeCache : IAttributeCache
    {
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        private readonly IObservable<EventPattern<AttributeEventArgs>> _attributes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AttributeCache"/> class.
        /// </summary>
        public AttributeCache()
        {
            _attributes = Observable.FromEventPattern<AttributeEventArgs>(
                h => AttributeChanged += h,
                h => AttributeChanged -= h);
        }

        private event EventHandler<AttributeEventArgs> AttributeChanged;

        /// <inheritdoc />
        public object this[string name]
        {
            get
            {
                if (!_cache.ContainsKey(name))
                {
                    throw new ArgumentException(@"The attribute does not exists", nameof(name));
                }

                return _cache[name];
            }

            set
            {
                if (!_cache.ContainsKey(name))
                {
                    throw new ArgumentException(@"The attribute does not exists", nameof(name));
                }

                _cache[name] = value;

                RaiseAttributeChanged(name, value);
            }
        }

        /// <inheritdoc />
        public bool TryAddAttribute(string name, object value)
        {
            var added = _cache.TryAdd(name, value);

            if (added)
            {
                RaiseAttributeChanged(name, value);
            }

            return added;
        }

        /// <inheritdoc />
        public bool ContainsAttribute(string name)
        {
            return _cache.ContainsKey(name);
        }

        /// <inheritdoc />
        public bool TryGetAttribute(string name, out object value)
        {
            return _cache.TryGetValue(name, out value);
        }

        /// <inheritdoc />
        public IDisposable Subscribe<TValue>(string name, IObserver<TValue> observer)
        {
            return _attributes
                .Where(e => e.EventArgs.Name == name)
                .Select(e => (TValue)e.EventArgs.Value)
                .ObserveOn(Scheduler.Default)
                .Subscribe(observer);
        }

        private void RaiseAttributeChanged(string name, object value)
        {
            AttributeChanged?.Invoke(this, new AttributeEventArgs(name, value));
        }
    }
}
