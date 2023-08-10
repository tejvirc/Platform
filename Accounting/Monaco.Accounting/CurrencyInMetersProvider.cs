namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Metering;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Provides meters pertaining to currency accepted by the EGM.
    /// </summary>
    public class CurrencyInMetersProvider : BaseMeterProvider, IDisposable
    {
        private const PersistenceLevel Level = PersistenceLevel.Static;
        private const PersistenceLevel ResetMeterLevel = PersistenceLevel.Critical;
        private const PersistenceLevel RejectedCountMeterLevel = PersistenceLevel.Critical;

        private const string AtomicCountMeterNamePrefix = "BillCount";
        private const string AtomicCountMeterNamePostfix = "s";
        private const string AtomicAmountMeterNamePrefix = "BillAmount";
        private const string AtomicAmountMeterNamePostfix = "s";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<Tuple<string, MeterClassification>> _rejectedCountMeters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(AccountingMeters.DocumentsRejectedCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.BillsRejectedCount, new OccurrenceMeterClassification())
        };

        private readonly IPropertiesManager _properties;
        private readonly INoteAcceptor _noteAcceptor;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IEventBus _eventBus;
        private readonly object _denomMeterLock = new object();
        private bool _disposed;

        private IList<int> _denominations = new List<int>();
        private Collection<IMeter> _meters = new Collection<IMeter>();
        private static double _multiplier;
        private DateTime _lastPeriodClearTime;

        // ReSharper disable once UnusedMember.Global
        public CurrencyInMetersProvider()
            : this(
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<INoteAcceptor>(),
                ServiceManager.GetInstance().GetService<IEventBus>()
                )
        {
        }

        public CurrencyInMetersProvider(
            IPersistentStorageManager storage,
            IPropertiesManager properties,
            INoteAcceptor noteAcceptor,
            IEventBus eventBus)
            : base(typeof(CurrencyInMetersProvider).ToString())
        {
            _persistentStorage = storage ?? throw new ArgumentNullException(nameof(storage));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));

            _noteAcceptor = noteAcceptor;

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<NoteUpdatedEvent>(this, HandleEvent);
            _eventBus.Subscribe<PersistentStorageClearedEvent>(this, HandleEvent);
            _eventBus.Subscribe<PeriodMetersClearedEvent>(this, HandleEvent);
            _eventBus.Subscribe<PropertyChangedEvent>(this, HandleEvent);

            InitializeMeters();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        /// <inheritdoc />
        public override DateTime LastPeriodClear
        {
            get => _lastPeriodClearTime;

            set
            {
                _lastPeriodClearTime = value;
                var blockName = GetType().ToString();
                var block = _persistentStorage.GetBlock(blockName);
                using (var transaction = block.StartTransaction())
                {
                    transaction["LastPeriodClearTime"] = value;
                    transaction.Commit();
                }
            }
        }

        private void InitializeMeters()
        {
            CreateUpdateDenomMeters();

            AddBillsCashInCompositeMeters(_meters);

            AddOrReplaceMeter(AddTotalCashInCompositeMeter(_meters));
            AddOrReplaceMeter(AddTotalBillCountCompositeMeter(_meters));

            AddOrReplaceRejectedCountMeters();

            // re-load this meter provider
            Invalidate();

            Logger.Info("InitializeMeters complete.");
        }

        private void AddOrReplaceRejectedCountMeters()
        {
            var accessor = _persistentStorage.GetAccessor(RejectedCountMeterLevel, Name);
            foreach (var meter in _rejectedCountMeters)
            {
                AddMeter(new AtomicMeter(meter.Item1, accessor, meter.Item2, this));
            }
        }

        private void CreateUpdateDenomMeters()
        {
            lock (_denomMeterLock)
            {
                var blockName = GetType().ToString();

                IPersistentStorageAccessor block = null;
                if (_persistentStorage.BlockExists(blockName))
                {
                    block = _persistentStorage.GetBlock(blockName);
                    VerifyBlock(block);
                    var persistedDenoms = GetPersistedDenominations(block);
                    _denominations = new List<int>(persistedDenoms);
                    _lastPeriodClearTime = (DateTime)block["LastPeriodClearTime"];
                }
                else
                {
                    var supportedDenoms = _noteAcceptor?.GetSupportedNotes() ?? new Collection<int>();
                    _denominations = new List<int>(supportedDenoms);

                    if (_denominations.Count > 0)
                    {
                        block = _persistentStorage.CreateBlock(Level, blockName, _denominations.Count);
                        StoreDenominationsInBlock(block, _denominations);
                    }
                }

                _multiplier = _properties.GetValue(ApplicationConstants.CurrencyMultiplierKey, 0D);


                var denomMeters = CreateDenominationMeters(block, _denominations);

                _meters = new Collection<IMeter>(denomMeters);
            }

            Logger.Debug(
                $"Denomination meters: local: {string.Join(",", _meters.Select(m => m.Name).ToList())}, base: {string.Join(",", MeterNames.ToList())}");
        }

        private static string GetDenominationMeterName(int denomination)
        {
            return AtomicCountMeterNamePrefix + denomination + AtomicCountMeterNamePostfix;
        }

        private static Collection<int> GetPersistedDenominations(IPersistentStorageAccessor block)
        {
            var denominations = new Collection<int>();

            // For each denomination, the denomination and an Atomic meter are stored
            var numDenominations = block.Count;

            var results = block.GetAll();

            // The set of denominations are stored first.
            for (var i = 0; i < numDenominations; ++i)
            {
                if (results.TryGetValue(i, out var data))
                {
                    denominations.Add((int)data["Denomination"]);
                }
            }

            return denominations;
        }

        private Collection<IMeter> CreateDenominationMeters(
            IPersistentStorageAccessor block,
            IList<int> denominations)
        {
            var meters = new Collection<IMeter>();

            var classification = new OccurrenceMeterClassification();

            for (var i = 0; i < denominations.Count; ++i)
            {
                var meter = _meters.SingleOrDefault(m => m.Name.Equals(GetDenominationMeterName(denominations[i])));
                if (meter == null)
                {
                    meter = new AtomicMeter(
                            GetDenominationMeterName(denominations[i]),
                            block,
                            i,
                            classification,
                        this);
                }

                meters.Add(meter);

                AddOrReplaceMeter(meter);
            }

            return meters;
        }

        private void VerifyBlock(IPersistentStorageAccessor block)
        {
            // Get denominations to provide meters for
            var denominations = _noteAcceptor?.GetSupportedNotes() ?? new Collection<int>();

            // Verify the block has data for the same denominations
            var blockDenominations = GetPersistedDenominations(block);

            bool addDenom = false;

            foreach (var denomination in denominations)
            {
                if (!blockDenominations.Contains(denomination))
                {
                    addDenom = true;
                    blockDenominations.Add(denomination);
                }
            }

            if (addDenom)
            {
                _persistentStorage.ResizeBlock(GetType().ToString(), denominations.Count);
                StoreDenominationsInBlock(block, denominations);
            }
        }

        private static void StoreDenominationsInBlock(IPersistentStorageAccessor block, IList<int> denominations)
        {
            for (var i = 0; i < denominations.Count; ++i)
            {
                block[i, "Denomination"] = denominations[i];
            }
        }

        private IMeter AddTotalCashInCompositeMeter(IReadOnlyList<IMeter> meters)
        {
            return new CompositeMeter(
                AccountingMeters.CurrencyInAmount,
                (timeFrame) =>
                {
                    var sum = 0L;
                    for (var denomIndex = 0; denomIndex < _denominations.Count; ++denomIndex)
                    {
                        sum += _meters[denomIndex].GetValue(timeFrame) * (long)(_denominations[denomIndex] * _multiplier);
                    }
                    return sum;
                },
                meters.Select(m => m.Name),
                new CurrencyMeterClassification());
        }

        private void AddBillsCashInCompositeMeters(IReadOnlyList<IMeter> meters)
        {
            var multiplier = _properties.GetValue(
                ApplicationConstants.CurrencyMultiplierKey,
                ApplicationConstants.DefaultCurrencyMultiplier);

            for (var denomIndex = 0; denomIndex < _denominations.Count; ++denomIndex)
            {
                var denominationValue = (long)(_denominations[denomIndex] * multiplier);

                var meter = meters[denomIndex];

                AddOrReplaceMeter(
                    new CompositeMeter(
                        $"{AtomicAmountMeterNamePrefix}{_denominations[denomIndex]}{AtomicAmountMeterNamePostfix}",
                        (timeFrame) => meter.GetValue(timeFrame) * denominationValue,
                        meters.Select(m => m.Name),
                        new CurrencyMeterClassification()));
            }
        }

        private void HandleEvent(PersistentStorageClearedEvent @event)
        {
            if (@event.Level > ResetMeterLevel)
            {
                return;
            }

            var blockName = GetType().ToString();

            if (_persistentStorage.BlockExists(blockName))
            {
                var block = _persistentStorage.GetBlock(blockName);
                var denominations = GetPersistedDenominations(block);

                for (var i = 0; i < denominations.Count; ++i)
                {
                    block[i, "Period"] = 0L;
                    block[i, "Lifetime"] = 0L;
                }

                block["LastPeriodClearTime"] = DateTime.MinValue;
            }
        }

        private void HandleEvent(NoteUpdatedEvent evt)
        {
            CreateUpdateDenomMeters();
        }

        private static IMeter AddTotalBillCountCompositeMeter(IReadOnlyCollection<IMeter> meters)
        {
            return new CompositeMeter(
                AccountingMeters.CurrencyInCount,
                (timeFrame) => { return meters.Sum(meter => meter.GetValue(timeFrame)); },
                meters.Select(m => m.Name),
                new OccurrenceMeterClassification());
        }
        
        private void HandleEvent(PeriodMetersClearedEvent evt)
        {
            // Only look for period clears that were specifically requested for this provider
            if (evt.ProviderName == GetType().ToString())
            {
                LastPeriodClear = DateTime.UtcNow;
            }
        }

        private void HandleEvent(PropertyChangedEvent evt)
        {
            if (evt.PropertyName != AccountingConstants.BillClearanceEnabled)
            {
                return;
            }
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();
            if ((bool)_properties.GetProperty(AccountingConstants.BillClearanceEnabled, false))
            {
                meterManager.ExemptProviderFromClearAllPeriodMeters(typeof(CurrencyInMetersProvider).ToString());
            }
        }
    }
}