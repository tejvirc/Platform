namespace Aristocrat.Monaco.Accounting.Handpay
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
    ///     Definition of the HandpayMetersProvider class.
    ///     Provides Handpay meters for the EGM.
    /// </summary>
    public class HandpayMetersProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedCancelNoReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedCancelReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedCancelNoReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedCancelReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedCancelNoReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedCancelReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedCancelNoReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedCancelReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedGameWinNoReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedGameWinReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedGameWinNoReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedGameWinReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedGameWinNoReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedGameWinReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedGameWinNoReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedGameWinReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedBonusPayNoReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedBonusPayReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedBonusPayNoReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedBonusPayReceiptAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedBonusPayNoReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidValidatedBonusPayReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedBonusPayNoReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNotValidatedBonusPayReceiptCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidPromoAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidNonCashableAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(AccountingMeters.HandpaidOutCount, new OccurrenceMeterClassification())
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayMetersProvider" /> class.
        /// </summary>
        public HandpayMetersProvider()
            : base(typeof(HandpayMetersProvider).ToString())
        {
            var persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            var blockName = GetType().ToString();

            AddMeters(persistentStorage.GetAccessor(Level, blockName));
            AddCompositeMeters();
        }

        private void AddCompositeMeters()
        {
            AddMeter(
                new CompositeMeter(
                    AccountingMeters.HandpaidCancelReceiptAmount,
                    (timeFrame) => GetMeter(AccountingMeters.HandpaidValidatedCancelReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedCancelReceiptAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.HandpaidValidatedCancelReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedCancelReceiptAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.HandpaidBonusPayReceiptAmount,
                    (timeFrame) => GetMeter(AccountingMeters.HandpaidValidatedBonusPayReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedBonusPayReceiptAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.HandpaidValidatedBonusPayReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedBonusPayReceiptAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.HandpaidGameWinReceiptAmount,
                    (timeFrame) => GetMeter(AccountingMeters.HandpaidValidatedGameWinReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedGameWinReceiptAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.HandpaidValidatedGameWinReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedGameWinReceiptAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.HandpaidCancelNoReceiptAmount,
                    (timeFrame) => GetMeter(AccountingMeters.HandpaidValidatedCancelNoReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedCancelNoReceiptAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.HandpaidValidatedCancelNoReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedCancelNoReceiptAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.HandpaidBonusPayNoReceiptAmount,
                    (timeFrame) => GetMeter(AccountingMeters.HandpaidValidatedBonusPayNoReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedBonusPayNoReceiptAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.HandpaidValidatedBonusPayNoReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedBonusPayNoReceiptAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.HandpaidGameWinNoReceiptAmount,
                    (timeFrame) => GetMeter(AccountingMeters.HandpaidValidatedGameWinNoReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedGameWinNoReceiptAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.HandpaidValidatedGameWinNoReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedGameWinNoReceiptAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.HandpaidCancelAmount,
                    (timeFrame) => GetMeter(AccountingMeters.HandpaidValidatedCancelNoReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidValidatedCancelReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedCancelNoReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedCancelReceiptAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.HandpaidValidatedCancelNoReceiptAmount,
                        AccountingMeters.HandpaidValidatedCancelReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedCancelNoReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedCancelReceiptAmount
                    }, 
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalHandpaidBonusPayAmount,
                    (timeFrame) => GetMeter(AccountingMeters.HandpaidValidatedBonusPayReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidValidatedBonusPayNoReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedBonusPayReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedBonusPayNoReceiptAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.HandpaidValidatedBonusPayReceiptAmount,
                        AccountingMeters.HandpaidValidatedBonusPayNoReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedBonusPayReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedBonusPayNoReceiptAmount,
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalHandpaidCredits,
                    (timeFrame) => GetMeter(AccountingMeters.HandpaidCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNonCashableAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidPromoAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.HandpaidCashableAmount,
                        AccountingMeters.HandpaidNonCashableAmount,
                        AccountingMeters.HandpaidPromoAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalHandpaidGameWonAmount,
                    (timeFrame) => GetMeter(AccountingMeters.HandpaidValidatedGameWinReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidValidatedGameWinNoReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedGameWinReceiptAmount).GetValue(timeFrame) +
                                   GetMeter(AccountingMeters.HandpaidNotValidatedGameWinNoReceiptAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        AccountingMeters.HandpaidValidatedGameWinReceiptAmount,
                        AccountingMeters.HandpaidValidatedGameWinNoReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedGameWinReceiptAmount,
                        AccountingMeters.HandpaidNotValidatedGameWinNoReceiptAmount
                    },
                    new CurrencyMeterClassification().Name));
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