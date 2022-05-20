namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using Communicator;
    using Kernel;
    using log4net;

    /// <summary>A device provider.</summary>
    /// <typeparam name="TAdapter">Type of the adapter.</typeparam>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.SharedDevice.IDeviceProvider{TAdapter}" />
    /// <seealso cref="T:Aristocrat.Monaco.Kernel.IService" />
    /// <seealso cref="T:System.IDisposable" />
    public abstract class DeviceProvider<TAdapter> : IDeviceProvider<TAdapter>,
        IService,
        IDisposable
        where TAdapter : IDeviceAdapter
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentDictionary<int, TAdapter> _adapters = new ConcurrentDictionary<int, TAdapter>();

        private bool _disposed;

        /// <inheritdoc />
        public TAdapter this[int key]
        {
            get
            {
                _adapters.TryGetValue(key, out var adapter);
                if (adapter == null)
                {
                    Logger.Error($"Adapter not found: {key}");
                }

                return adapter;
            }
        }

        /// <inheritdoc />
        public bool Initialized { get; protected set; }

        /// <inheritdoc />
        public IEnumerable<TAdapter> Adapters => _adapters.Values;

        /// <inheritdoc />
        public void ClearAdapters()
        {
            _adapters.Clear();
        }

        /// <inheritdoc />
        public IDevice DeviceConfiguration(int deviceId)
        {
            _adapters.TryGetValue(deviceId, out var adapter);
            return adapter?.DeviceConfiguration;
        }

        /// <inheritdoc />
        public abstract TAdapter CreateAdapter(string name);

        /// <inheritdoc />
        public void Inspect(int deviceId, IComConfiguration config, int timeout)
        {
            _adapters.TryGetValue(deviceId, out var adapter);
            adapter?.Inspect(config, timeout);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract ICollection<Type> ServiceTypes { get; }

        /// <inheritdoc />
        public void Initialize()
        {
            foreach (var item in Adapters)
            {
                if (!item.Initialized)
                {
                    item.Initialize();
                }
            }

            Initialized = true;
        }

        /// <summary>Inserts an adapter.</summary>
        /// <param name="adapter">The adapter.</param>
        /// <param name="key">The key.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        protected bool InsertAdapter(TAdapter adapter, int key)
        {
            return _adapters.TryAdd(key, adapter);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the
        ///     Aristocrat.Monaco.Hardware.Contracts.SharedDevice.DeviceProvider&lt;TAdapter&gt; and optionally releases the
        ///     managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var adapter in Adapters)
                {
                    if (adapter is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                _adapters.Clear();
            }

            _disposed = false;
        }
    }
}