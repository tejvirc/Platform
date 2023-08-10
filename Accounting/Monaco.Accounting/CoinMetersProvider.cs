namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Metering;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    
    /// <summary>
    ///     Provides coin meters pertaining to currency accepted by the EGM.
    /// </summary>
    public sealed class CoinMetersProvider : BaseMeterProvider, IDisposable
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IEventBus _eventBus;
        private readonly long _multiplier;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(AccountingMeters.TrueCoinInCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.TrueCoinOutCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.CoinToCashBoxCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.CoinToHopperCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.CoinToCashBoxInsteadHopperCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.CoinToHopperInsteadCashBoxCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HopperRefillCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HopperRefillAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.ExcessCoinOutCount, new OccurrenceMeterClassification())
        };

        // ReSharper disable once UnusedMember.Global
        public CoinMetersProvider()
            : this(
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        [CLSCompliant(false)]
        public CoinMetersProvider(
            IPersistentStorageManager storage,
            IPropertiesManager properties,
            IEventBus eventBus)
            : base(typeof(CoinMetersProvider).ToString())
        {
            _persistentStorage = storage ?? throw new ArgumentNullException(nameof(storage));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _multiplier = _properties.GetValue(HardwareConstants.CoinValue, AccountingConstants.DefaultTokenValue);
            Init();
        }

        public void Init()
        {
            var blockName = GetType().ToString();

            AddMeters(_persistentStorage.GetAccessor(Level, blockName));

            AddCompositeMeters();
        }

        private void AddMeters(IPersistentStorageAccessor block)
        {
            foreach (var meter in _meters)
            {
                AddMeter(new AtomicMeter(meter.Item1, block, meter.Item2, this));
            }
        }

        private void AddCompositeMeters()
        {
            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TrueCoinIn,
                    (timeFrame) => GetMeter(AccountingMeters.TrueCoinInCount).GetValue(timeFrame) * _multiplier,
                    new List<string>
                    {
                        AccountingMeters.TrueCoinInCount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TrueCoinOut,
                    (timeFrame) => GetMeter(AccountingMeters.TrueCoinOutCount).GetValue(timeFrame) * _multiplier,
                    new List<string>
                    {
                        AccountingMeters.TrueCoinOutCount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.CoinDrop,
                    timeFrame =>
                    {
                        return GetMeter(AccountingMeters.CoinsToCashBox).GetValue(timeFrame) +
                               GetMeter(AccountingMeters.CoinsToCashBoxInsteadHopper).GetValue(timeFrame);
                    },
                    new List<string>
                    {
                        AccountingMeters.CoinsToCashBox,
                        AccountingMeters.CoinsToCashBoxInsteadHopper
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.CoinsToCashBox,
                    (timeFrame) => GetMeter(AccountingMeters.CoinToCashBoxCount).GetValue(timeFrame) * _multiplier,
                    new List<string>
                    {
                        AccountingMeters.CoinToCashBoxCount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.CoinsToHopper,
                    (timeFrame) => GetMeter(AccountingMeters.CoinToHopperCount).GetValue(timeFrame) * _multiplier,
                    new List<string>
                    {
                        AccountingMeters.CoinToHopperCount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.CoinsToCashBoxInsteadHopper,
                    (timeFrame) => GetMeter(AccountingMeters.CoinToCashBoxInsteadHopperCount).GetValue(timeFrame) * _multiplier,
                    new List<string>
                    {
                        AccountingMeters.CoinToCashBoxInsteadHopperCount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.CoinsToHopperInsteadCashBox,
                    (timeFrame) => GetMeter(AccountingMeters.CoinToHopperInsteadCashBoxCount).GetValue(timeFrame) * _multiplier,
                    new List<string>
                    {
                        AccountingMeters.CoinToHopperInsteadCashBoxCount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.CurrentHopperLevelCount,
                    (timeFrame) => GetMeter(AccountingMeters.CoinToHopperCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HopperRefillAmount).GetValue(timeFrame) / _multiplier -
                                   GetMeter(AccountingMeters.TrueCoinOutCount).GetValue(timeFrame) -
                                   GetMeter(AccountingMeters.ExcessCoinOutCount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.CoinToHopperCount,
                        AccountingMeters.HopperRefillAmount,
                        AccountingMeters.TrueCoinOutCount,
                        AccountingMeters.ExcessCoinOutCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.CurrentHopperLevel,
                    (timeFrame) => GetMeter(AccountingMeters.CurrentHopperLevelCount).GetValue(timeFrame) * _multiplier,
                    new List<string>
                    {
                        AccountingMeters.CurrentHopperLevelCount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.ExcessCoinOutAmount,
                    (timeFrame) => GetMeter(AccountingMeters.ExcessCoinOutCount).GetValue(timeFrame) * _multiplier,
                    new List<string>
                    {
                        AccountingMeters.ExcessCoinOutCount
                    },
                    new CurrencyMeterClassification()));
        }

        public void Dispose()
        {
            _eventBus.UnsubscribeAll(this);
        }
    }
}
