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
    ///     Provides Voucher in meters for the EGM
    /// </summary>
    public class VoucherInMetersProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherInCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherInCashableCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherInCashablePromoAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherInCashablePromoCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherInNonCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherInNonCashableCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherInNonTransferableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.VoucherInNonTransferableCount, new OccurrenceMeterClassification())
        };

        public VoucherInMetersProvider()
            : this(ServiceManager.GetInstance().GetService<IPersistentStorageManager>())
        {
        }

        public VoucherInMetersProvider(IPersistentStorageManager storage)
            : base(typeof(VoucherInMetersProvider).ToString())
        {
            AddMeters(storage.GetAccessor(Level, Name));

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
                    AccountingMeters.TotalVouchersIn,
                    (timeFrame) => GetMeter(AccountingMeters.VoucherInCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherInCashablePromoAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherInNonCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherInNonTransferableAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.VoucherInCashableAmount,
                        AccountingMeters.VoucherInCashablePromoAmount,
                        AccountingMeters.VoucherInNonCashableAmount,
                        AccountingMeters.VoucherInNonTransferableAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalVouchersInCount,
                    (timeFrame) => GetMeter(AccountingMeters.VoucherInCashableCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherInCashablePromoCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherInNonCashableCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherInNonTransferableCount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.VoucherInCashableCount,
                        AccountingMeters.VoucherInCashablePromoCount,
                        AccountingMeters.VoucherInNonCashableCount,
                        AccountingMeters.VoucherInNonTransferableCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalVoucherInCashableAndPromoAmount,
                    (timeFrame) => GetMeter(AccountingMeters.VoucherInCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherInCashablePromoAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.VoucherInCashableAmount,
                        AccountingMeters.VoucherInCashablePromoAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalVoucherInCashableAndPromoCount,
                    (timeFrame) => GetMeter(AccountingMeters.VoucherInCashableCount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.VoucherInCashablePromoCount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.VoucherInCashableCount,
                        AccountingMeters.VoucherInCashablePromoCount
                    },
                    new OccurrenceMeterClassification()));
        }
    }
}