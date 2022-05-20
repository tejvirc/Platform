namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;

    public class PlayerMeterProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(PlayerMeters.CardedBonusWonAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(PlayerMeters.CardedGameWonAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(PlayerMeters.CardedPlayedCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(PlayerMeters.CardedProgressiveWonAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(PlayerMeters.CardedWageredCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(PlayerMeters.CardedWageredNonCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(PlayerMeters.CardedWageredPromoAmount, new CurrencyMeterClassification()),
        };

        public PlayerMeterProvider(IPersistentStorageManager storageManager)
            : base(typeof(PlayerMeterProvider).ToString())
        {
            var blockName = GetType().ToString();

            AddMeters(storageManager.GetAccessor(Level, blockName));
        }

        private void AddMeters(IPersistentStorageAccessor accessor)
        {
            foreach (var meter in _meters)
            {
                AddMeter(new AtomicMeter(meter.Item1, accessor, meter.Item2, this));
            }
        }
    }
}