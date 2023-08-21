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
    ///     Provides wat on meters for the EGM.
    /// </summary>
    public class WatOnMetersProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOnCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOnCashableCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOnCashablePromoAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOnCashablePromoCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOnNonCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOnNonCashableCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOnCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOnCashableCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOnCashablePromoAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOnCashablePromoCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOnNonCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOnNonCashableCount, new OccurrenceMeterClassification())
        };

        public WatOnMetersProvider()
            : this(ServiceManager.GetInstance().GetService<IPersistentStorageManager>())
        {
        }

        public WatOnMetersProvider(IPersistentStorageManager storage)
            : base(typeof(WatOnMetersProvider).ToString())
        {
            var blockName = GetType().ToString();

            AddMeters(storage.GetAccessor(Level, blockName));

            AddCompositeMeters();
        }

        private void AddMeters(IPersistentStorageAccessor accessor)
        {
            foreach (var meter in _meters)
            {
                AddMeter(new AtomicMeter(meter.Item1, accessor, meter.Item2, this));
            }
        }

        private void AddCompositeMeters()
        {
            AddMeter(
                new CompositeMeter(
                    AccountingMeters.WatOnTotalAmount,
                    (timeFrame) => GetMeter(AccountingMeters.WatOnCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.WatOnCashablePromoAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.WatOnNonCashableAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.WatOnCashableAmount,
                        AccountingMeters.WatOnCashablePromoAmount,
                        AccountingMeters.WatOnNonCashableAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.WatOnTotalCount,
                    (timeFrame) => GetMeter(AccountingMeters.WatOnCashableCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.WatOnCashablePromoCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.WatOnNonCashableCount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.WatOnCashableCount,
                        AccountingMeters.WatOnCashablePromoCount,
                        AccountingMeters.WatOnNonCashableCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.ElectronicTransfersOnTotalAmount,
                    (timeFrame) => GetMeter(AccountingMeters.ElectronicTransfersOnCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.ElectronicTransfersOnCashablePromoAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.ElectronicTransfersOnNonCashableAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.ElectronicTransfersOnCashableAmount,
                        AccountingMeters.ElectronicTransfersOnCashablePromoAmount,
                        AccountingMeters.ElectronicTransfersOnNonCashableAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.ElectronicTransfersOnCashableAmount,
                    (timeFrame) => GetMeter(AccountingMeters.WatOnCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.KeyedOnCashableAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.WatOnCashableAmount,
                        AccountingMeters.KeyedOnCashableAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.ElectronicTransfersOnCashablePromoAmount,
                    (timeFrame) => GetMeter(AccountingMeters.WatOnCashablePromoAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.KeyedOnCashablePromoAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.WatOnCashablePromoAmount,
                        AccountingMeters.KeyedOnCashablePromoAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.ElectronicTransfersOnNonCashableAmount,
                    (timeFrame) => GetMeter(AccountingMeters.WatOnNonCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.KeyedOnNonCashableAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.WatOnNonCashableAmount,
                        AccountingMeters.KeyedOnNonCashableAmount
                    },
                    new CurrencyMeterClassification()));
        }
    }
}