namespace Aristocrat.Monaco.Accounting.Hopper
{
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Provides test coin meters pertaining to currency accepted by the EGM.
    /// </summary>
    public class CoinOutTestMetersProvider : BaseMeterProvider, IDisposable
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IEventBus _eventBus;
        private DateTime _lastPeriodClearTime;
        private bool _disposed;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
            {
                Tuple.Create<string, MeterClassification>(AccountingMeters.HopperTestCoinsIn, new OccurrenceMeterClassification()),
                Tuple.Create<string, MeterClassification>(AccountingMeters.HopperTestCoinsOut, new OccurrenceMeterClassification()),
                Tuple.Create<string, MeterClassification>(AccountingMeters.ExtraCoinInWhenHopperTest, new CurrencyMeterClassification()),
                Tuple.Create<string, MeterClassification>(AccountingMeters.ExtraCoinOutWhenHopperTest, new OccurrenceMeterClassification())
            };

        // ReSharper disable once UnusedMember.Global
        public CoinOutTestMetersProvider()
            : this(
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>()
                )
        {
        }

        [CLSCompliant(false)]
        public CoinOutTestMetersProvider(
            IPersistentStorageManager storage,
            IEventBus eventBus)
            : base(typeof(CoinOutTestMetersProvider).ToString())
        {
            _persistentStorage = storage ?? throw new ArgumentNullException(nameof(storage));

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.Subscribe<PeriodMetersClearedEvent>(this, HandleEvent);

            Init();
        }

        public void Init()
        {
            var blockName = GetType().ToString();

            AddMeters(_persistentStorage.GetAccessor(Level, blockName));
        }

        private void AddMeters(IPersistentStorageAccessor block)
        {
            foreach (var meter in _meters)
            {
                AddMeter(new AtomicMeter(meter.Item1, block, meter.Item2, this));
            }
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

        private void HandleEvent(PeriodMetersClearedEvent evt)
        {
            // Only look for period clears that were specifically requested for this provider
            if (evt.ProviderName == GetType().ToString())
            {
                LastPeriodClear = DateTime.UtcNow;
            }
        }
    }
}
