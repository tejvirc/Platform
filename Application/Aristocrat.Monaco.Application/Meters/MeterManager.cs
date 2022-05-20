namespace Aristocrat.Monaco.Application.Meters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Common;
    using Contracts;
    using Contracts.Extensions;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Mono.Addins;
    using MeterSnapshot = Contracts.MeterSnapshot;

    /// <summary>
    ///     Manages virtual, persisted and non-persisted meters.
    /// </summary>
    public sealed class MeterManager : IMeterManager, IDisposable
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;
        private const string MeterProvidersExtensionPoint = "/Application/Metering/Providers";

        private static readonly string PersistedBlockName = MethodBase.GetCurrentMethod().DeclaringType?.ToString();
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentDictionary<string, IMeterProvider> _meterMap =
            new ConcurrentDictionary<string, IMeterProvider>();

        private readonly HashSet<string> _providersExemptFromClearAllPeriodMeters = new HashSet<string>();

        private readonly IPersistentStorageManager _storage;
        private readonly IEventBus _bus;
        private readonly IPropertiesManager _properties;

        private bool _disposed;

        private DateTime _lastMasterClearTime;
        private DateTime _lastPeriodClearTime;

        public MeterManager()
            : this(
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public MeterManager(IPersistentStorageManager storage, IEventBus bus, IPropertiesManager properties)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        private IEnumerable<IMeterProvider> Providers => _meterMap.Values.DistinctBy(k => k.Name);

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var provider in Providers)
            {
                foreach (var meterName in provider.MeterNames)
                {
                    var meter = provider.GetMeter(meterName) as CompositeMeter;
                    meter?.Dispose();
                }
            }

            _meterMap.Clear();

            _disposed = true;
        }

        /// <inheritdoc />
        public IEnumerable<string> Meters => _meterMap.Keys;

        /// <inheritdoc />
        public DateTime LastPeriodClear
        {
            get => _lastPeriodClearTime;

            private set
            {
                _lastPeriodClearTime = value;

                var block = _storage.GetBlock(PersistedBlockName);
                using (var transaction = block.StartTransaction())
                {
                    transaction["LastPeriodClearTime"] = value;
                    transaction.Commit();
                }
            }
        }

        /// <inheritdoc />
        public DateTime LastMasterClear
        {
            get => _lastMasterClearTime;

            private set
            {
                _lastMasterClearTime = value;

                var block = _storage.GetBlock(PersistedBlockName);
                using (var transaction = block.StartTransaction())
                {
                    transaction["LastMasterClearTime"] = value;
                    transaction.Commit();
                }
            }
        }

        /// <inheritdoc />
        public void AddProvider(IMeterProvider provider)
        {
            if (Providers.Any(p => p.Name == provider.Name))
            {
                var message = $"{provider.Name} already added";
                Logger.Fatal(message);
                throw CreateMeterException(message, null);
            }

            foreach (var meterName in provider.MeterNames)
            {
                if (_meterMap.ContainsKey(meterName))
                {
                    var message = $"{meterName} already provided";
                    Logger.Fatal(message);
                    throw CreateMeterException(message, null);
                }

                _meterMap.TryAdd(meterName, provider);
            }

            InitializeCompositeMeters(provider);

            _bus.Publish(new MeterProviderAddedEvent());
        }

        public void InvalidateProvider(IMeterProvider provider)
        {
            Logger.Debug($"InvalidateProvider: {provider.Name}");

            foreach (var meterName in provider.MeterNames)
            {
                if (!_meterMap.ContainsKey(meterName))
                {
                    _meterMap.TryAdd(meterName, provider);
                }
            }

            InitializeCompositeMeters(provider);
        }

        /// <inheritdoc />
        public IEnumerable<string> MetersProvided(string providerName)
        {
            var provider = GetProvider(providerName, false);
            return provider == null ? Enumerable.Empty<string>() : provider.MeterNames;
        }

        /// <inheritdoc />
        public bool IsMeterProvided(string meterName)
        {
            return _meterMap.ContainsKey(meterName);
        }

        /// <inheritdoc />
        public void ClearAllPeriodMeters()
        {
            using (var scopedTransaction = _storage.ScopedTransaction())
            {
                foreach (var provider in Providers)
                {
                    if (_providersExemptFromClearAllPeriodMeters.Contains(provider.Name))
                    {
                        Logger.Debug($"Didn't clear period meters for provider because it was exempt: {provider.Name}");
                        continue;
                    }
                    Logger.Debug($"Clearing period meters for provider: {provider.Name}");
                    provider.ClearPeriodMeters();
                }

                LastPeriodClear = DateTime.UtcNow;

                scopedTransaction.Complete();
            }

            _bus.Publish(new PeriodMetersClearedEvent());
        }

        private IMeterProvider GetProvider(string providerName, bool throwIfNotFound = true)
        {
            var provider = Providers.FirstOrDefault(p => p.Name == providerName);
            if (provider == null && throwIfNotFound)
                throw new ArgumentException($"Trying to find provider with providerName {providerName} doesn't exist as a Meter Provider.");
            return provider;
        }

        /// <inheritdoc />
        public void ClearPeriodMeters(string providerName)
        {
            var provider = GetProvider(providerName);
            using (var scopedTransaction = _storage.ScopedTransaction())
            {
                Logger.Debug($"Clearing period meters ONLY for the provider: {provider.Name}");
                provider.ClearPeriodMeters();
                scopedTransaction.Complete();
            }

            _bus.Publish(new PeriodMetersClearedEvent(provider.Name));
        }

        public void ExemptProviderFromClearAllPeriodMeters(string providerName)
        {
            Logger.Debug($"Exempting Meter Provider from ClearAllPeriodMeters: {providerName}");
            var provider = GetProvider(providerName);
            _providersExemptFromClearAllPeriodMeters.Add(provider.Name);
        }

        /// <inheritdoc />
        public IMeter GetMeter(string meterName)
        {
            if (_meterMap.TryGetValue(meterName, out var provider))
            {
                return provider.GetMeter(meterName);
            }

            var exceptionMessage = $"GetMeter() failed to retrieve meter: {meterName} -- meter not found.";
            Logger.Fatal(exceptionMessage);

            throw new MeterNotFoundException(exceptionMessage);
        }

        /// <inheritdoc />
        public Dictionary<string, MeterSnapshot> CreateSnapshot()
        {
            Logger.Info("Creating meter snapshots");

            var meterSnapshots = new Dictionary<string, MeterSnapshot>();

            foreach (var provider in Providers)
            {
                var providerSnapshot = CreateSnapshot(provider);
                foreach (var snapshot in providerSnapshot)
                {
                    meterSnapshots[snapshot.Key] = snapshot.Value;
                }
            }

            Logger.Info("Creating meter snapshots...complete!");

            return meterSnapshots;
        }

        public Dictionary<string, long> CreateSnapshot(MeterValueType valueType)
        {
            var currentSnapshot = CreateSnapshot();

            return currentSnapshot.ToDictionary(k => k.Key, v => v.Value.Values[valueType]);
        }

        public Dictionary<string, MeterSnapshot> CreateSnapshot(IEnumerable<string> meterNames)
        {
            var meterSnapshots = new Dictionary<string, MeterSnapshot>();

            foreach(var meterName in meterNames)
            {

                var meter = GetMeter(meterName);

                meterSnapshots[meterName] = new MeterSnapshot
                {
                    Name = meterName,
                    Values = new Dictionary<MeterValueType, long>
                {
                    { MeterValueType.Lifetime, meter.Lifetime },
                    { MeterValueType.Period, meter.Period },
                    { MeterValueType.Session, meter.Session }
                }
                };
            }
            return meterSnapshots;
        }

        public Dictionary<string, long> CreateSnapshot(IEnumerable<string> meters, MeterValueType valueType)
        {
            var currentSnapshot = CreateSnapshot(meters);

            return currentSnapshot.ToDictionary(k => k.Key, v => v.Value.Values[valueType]);
        }

        public IDictionary<string, long> GetSnapshotDelta(
            IDictionary<string, MeterSnapshot> snapshot,
            MeterValueType valueType)
        {
            var result = new Dictionary<string, long>();

            var currentSnapshot = CreateSnapshot();

            foreach (var current in currentSnapshot)
            {
                if (snapshot.TryGetValue(current.Key, out var previous))
                {
                    var currentValue = current.Value.Values[valueType];
                    var previousValue = previous.Values[valueType];

                    if (currentValue != previousValue)
                    {
                        result.Add(current.Key, currentValue - previousValue);
                    }
                }
            }

            return result;
        }

        public IDictionary<string, long> GetSnapshotDelta(IDictionary<string, long> snapshot, MeterValueType valueType)
        {
            var result = new Dictionary<string, long>();

            var currentSnapshot = CreateSnapshot(valueType);

            foreach (var current in currentSnapshot)
            {
                if (snapshot.TryGetValue(current.Key, out var previous))
                {
                    if (current.Value != previous)
                    {
                        result.Add(current.Key, current.Value - previous);
                    }
                }
            }

            return result;
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IMeterManager) };

        /// <inheritdoc />
        public void Initialize()
        {
            // Get or create block of persistent storage
            if (_storage.BlockExists(PersistedBlockName))
            {
                var block = _storage.GetBlock(PersistedBlockName);
                _lastPeriodClearTime = (DateTime)block["LastPeriodClearTime"];
                _lastMasterClearTime = (DateTime)block["LastMasterClearTime"];
            }
            else
            {
                _storage.CreateBlock(Level, PersistedBlockName, 1);
                LastPeriodClear = DateTime.UtcNow;
                LastMasterClear = DateTime.UtcNow;
            }

            // Load MeterProvider add-ins.  They only need to be constructed, during
            // which they should call back to register the meters they create.
            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes(MeterProvidersExtensionPoint))
            {
                Logger.Debug($"Creating Meter Provider: {node.Type}");

                var provider = (IMeterProvider)node.CreateInstance();
                AddProvider(provider);
            }

            var genericCompositeMeterProvider = new CompositeMetersProvider();

            AddProvider(genericCompositeMeterProvider);
            InitializeCompositeMeters(genericCompositeMeterProvider);

            // ReSharper disable once StringLiteralTypo
            if (_properties.GetValue("maxmeters", "false") == "true")
            {
                SetupMeterRolloverTest();
            }
        }

        private static Exception CreateMeterException(string message, Exception insideException)
        {
            Logger.Fatal(message);

            return insideException == null ? new MeterException(message) : new MeterException(message, insideException);
        }

        private static Dictionary<string, MeterSnapshot> CreateSnapshot(IMeterProvider provider)
        {
            var meters = new List<string>(provider.MeterNames);
            var meterSnapshots = new Dictionary<string, MeterSnapshot>(meters.Count);

            foreach (var meterName in meters)
            {
                var meter = provider.GetMeter(meterName);

                meterSnapshots[meterName] = new MeterSnapshot
                {
                    Name = meterName,
                    Values = new Dictionary<MeterValueType, long>
                    {
                        { MeterValueType.Lifetime, meter.Lifetime },
                        { MeterValueType.Period, meter.Period },
                        { MeterValueType.Session, meter.Session }
                    }
                };
            }

            return meterSnapshots;
        }

        private void InitializeCompositeMeters(IMeterProvider provider)
        {
            foreach (var meterName in provider.MeterNames)
            {
                var meter = provider.GetMeter(meterName) as CompositeMeter;
                meter?.Initialize(this);
            }
        }

        private void SetupMeterRolloverTest()
        {
            // First, make sure all meters are zero; i.e. machine has just been initialized and never played before
            var allZero = true;
            foreach (var meterName in Meters)
            {
                if (GetMeter(meterName) is AtomicMeter meter && meter.Lifetime != 0)
                {
                    allZero = false;
                    break;
                }
            }

            if (!allZero)
            {
                return;
            }

            var currencyMultiplier = (double)_properties.GetProperty(
                ApplicationConstants.CurrencyMultiplierKey,
                ApplicationConstants.DefaultCurrencyMultiplier);
            var oneCent = (long)currencyMultiplier / (int)CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit;

            foreach (var meterName in Meters)
            {
                if (!(GetMeter(meterName) is AtomicMeter meter))
                {
                    continue;
                }

                var preRollover = meter.Classification.UpperBounds;
                if (meter.Classification.GetType() == typeof(CurrencyMeterClassification))
                {
                    preRollover -= oneCent;
                }
                else
                {
                    preRollover -= 1;
                }

                Logger.Debug($"Incrementing meter: {meterName} to {preRollover}");
                if (meter.Lifetime == 0)
                {
                    meter.Increment(preRollover);
                }
            }
        }

        /// <inheritdoc />
        public DateTime GetPeriodMetersClearanceDate(string providerName)
        {
            var provider = GetProvider(providerName);
            return provider.LastPeriodClear != DateTime.MinValue ? provider.LastPeriodClear : LastPeriodClear;
        }
    }
}