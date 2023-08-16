namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Metering;
    using Contracts;
    using Contracts.Bonus;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     Provides cabinet specific meters related to game play
    /// </summary>
    public class CabinetMeterProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<Tuple<string, MeterClassification, bool>> _meters =
            new()
            {
                Tuple.Create<string, MeterClassification, bool>(
                    GamingMeters.WageredCashableAmount,
                    new CurrencyMeterClassification(),
                    true),
                Tuple.Create<string, MeterClassification, bool>(
                    GamingMeters.WageredPromoAmount,
                    new CurrencyMeterClassification(),
                    true),
                Tuple.Create<string, MeterClassification, bool>(
                    GamingMeters.WageredNonCashableAmount,
                    new CurrencyMeterClassification(),
                    true),
                Tuple.Create<string, MeterClassification, bool>(
                    GamingMeters.GamesPlayedSinceDoorClosed,
                    new OccurrenceMeterClassification(),
                    true),
                Tuple.Create<string, MeterClassification, bool>(
                    GamingMeters.GamesPlayedSinceDoorOpen,
                    new OccurrenceMeterClassification(),
                    true),
                Tuple.Create<string, MeterClassification, bool>(
                    GamingMeters.GamesPlayedSinceReboot,
                    new OccurrenceMeterClassification(),
                    false)
            };

        private readonly IMeterManager _meterManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CabinetMeterProvider" /> class.
        /// </summary>
        public CabinetMeterProvider(IPersistentStorageManager persistentStorage, IMeterManager meterManager, IPropertiesManager properties)
            : base(typeof(CabinetMeterProvider).ToString(), properties)
        {
            if (persistentStorage == null)
            {
                throw new ArgumentNullException(nameof(persistentStorage));
            }

            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            var blockName = GetType().ToString();
            var accessor = persistentStorage.BlockExists(blockName)
                ? persistentStorage.GetBlock(blockName)
                : persistentStorage.CreateBlock(Level, blockName, 1);

            AddMeters(accessor);
            AddCompositeMeters();
            _meterManager.AddProvider(this);
        }

        private void AddMeters(IPersistentStorageAccessor accessor)
        {
            foreach (var meter in _meters)
            {
                if (meter.Item3)
                {
                    AddMeter(new AtomicMeter(meter.Item1, accessor, meter.Item2, this));
                }
                else
                {
                    AddMeter(new NonPersistentAtomicMeter(meter.Item1, meter.Item2, this));
                }
            }
        }

        private void AddCompositeMeters()
        {
            AddMeter(
                new CompositeMeter(
                    GamingMeters.TotalJackpotWonCount,
                    timeFrame => _meterManager.GetMeter(GamingMeters.HandPaidGameWonCount).GetValue(timeFrame) +
                                 _meterManager.GetMeter(BonusMeters.HandPaidBonusCount).GetValue(timeFrame) +
                                 _meterManager.GetMeter(GamingMeters.HandPaidProgWonCount).GetValue(timeFrame),
                    new List<string> { GamingMeters.HandPaidGameWonCount, BonusMeters.HandPaidBonusCount, GamingMeters.HandPaidProgWonCount },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    GamingMeters.TotalProgWonCount,
                    timeFrame => _meterManager.GetMeter(GamingMeters.EgmPaidProgWonCount).GetValue(timeFrame) +
                                 _meterManager.GetMeter(GamingMeters.HandPaidProgWonCount).GetValue(timeFrame),
                    new List<string> { GamingMeters.EgmPaidProgWonCount, GamingMeters.HandPaidProgWonCount },
                    new OccurrenceMeterClassification()));
        }
    }
}