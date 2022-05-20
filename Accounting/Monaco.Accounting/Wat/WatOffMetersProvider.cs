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
    ///     Provides wat off meters for the EGM.
    /// </summary>
    public class WatOffMetersProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOffCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOffCashableCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOffCashablePromoAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOffCashablePromoCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOffNonCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.WatOffNonCashableCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOffCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOffCashableCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOffCashablePromoAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOffCashablePromoCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOffNonCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.KeyedOffNonCashableCount, new OccurrenceMeterClassification()),
        };

        public WatOffMetersProvider()
            : this(ServiceManager.GetInstance().GetService<IPersistentStorageManager>())
        {
        }

        public WatOffMetersProvider(IPersistentStorageManager storage)
            : base(typeof(WatOffMetersProvider).ToString())
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
                    AccountingMeters.WatOffTotalAmount,
                    (timeFrame) => GetMeter(AccountingMeters.WatOffCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.WatOffCashablePromoAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.WatOffNonCashableAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.WatOffCashableAmount,
                        AccountingMeters.WatOffCashablePromoAmount,
                        AccountingMeters.WatOffNonCashableAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.WatOffTotalCount,
                    (timeFrame) => GetMeter(AccountingMeters.WatOffCashableCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.WatOffCashablePromoCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.WatOffNonCashableCount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.WatOffCashableCount,
                        AccountingMeters.WatOffCashablePromoCount,
                        AccountingMeters.WatOffNonCashableCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.ElectronicTransfersOffTotalAmount,
                    (timeFrame) => GetMeter(AccountingMeters.ElectronicTransfersOffCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.ElectronicTransfersOffCashablePromoAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.ElectronicTransfersOffNonCashableAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.ElectronicTransfersOffCashableAmount,
                        AccountingMeters.ElectronicTransfersOffCashablePromoAmount,
                        AccountingMeters.ElectronicTransfersOffNonCashableAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.ElectronicTransfersOffCashableAmount,
                    (timeFrame) => GetMeter(AccountingMeters.WatOffCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.KeyedOffCashableAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.WatOffCashableAmount,
                        AccountingMeters.KeyedOffCashableAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.ElectronicTransfersOffCashablePromoAmount,
                    (timeFrame) => GetMeter(AccountingMeters.WatOffCashablePromoAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.KeyedOffCashablePromoAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.WatOffCashablePromoAmount,
                        AccountingMeters.KeyedOffCashablePromoAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.ElectronicTransfersOffNonCashableAmount,
                    (timeFrame) => GetMeter(AccountingMeters.WatOffNonCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.KeyedOffNonCashableAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.WatOffNonCashableAmount,
                        AccountingMeters.KeyedOffNonCashableAmount
                    },
                    new CurrencyMeterClassification()));
        }
    }
}