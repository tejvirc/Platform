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
    ///     Provides Voucher out meters for the EGM
    /// </summary>
    public class VoucherOutMetersProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherOutCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherOutCashableCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherOutCashablePromoAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherOutCashablePromoCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherOutNonCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherOutNonCashableCount, new OccurrenceMeterClassification())
        };

        public VoucherOutMetersProvider()
            : this(ServiceManager.GetInstance().GetService<IPersistentStorageManager>())
        {
        }

        public VoucherOutMetersProvider(IPersistentStorageManager storage)
            : base(typeof(VoucherOutMetersProvider).ToString())
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
                    AccountingMeters.TotalVouchersOut,
                    (timeFrame) => GetMeter(AccountingMeters.VoucherOutCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherOutCashablePromoAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherOutNonCashableAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.VoucherOutCashableAmount,
                        AccountingMeters.VoucherOutCashablePromoAmount,
                        AccountingMeters.VoucherOutNonCashableAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalVouchersOutCount,
                    (timeFrame) => GetMeter(AccountingMeters.VoucherOutCashableCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherOutCashablePromoCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherOutNonCashableCount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.VoucherOutCashableCount,
                        AccountingMeters.VoucherOutCashablePromoCount,
                        AccountingMeters.VoucherOutNonCashableCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalVoucherOutCashableAndPromoAmount,
                    (timeFrame) => GetMeter(AccountingMeters.VoucherOutCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherOutCashablePromoAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.VoucherOutCashableAmount,
                        AccountingMeters.VoucherOutCashablePromoAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalVoucherOutCashableAndPromoCount,
                    (timeFrame) => GetMeter(AccountingMeters.VoucherOutCashableCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherOutCashablePromoCount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.VoucherOutCashableCount,
                        AccountingMeters.VoucherOutCashablePromoCount
                    },
                    new OccurrenceMeterClassification()));
        }
    }
}