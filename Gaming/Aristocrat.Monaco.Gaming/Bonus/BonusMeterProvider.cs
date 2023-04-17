namespace Aristocrat.Monaco.Gaming.Bonus
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Metering;
    using Contracts;
    using Contracts.Bonus;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Provides Voucher out meters for the EGM
    /// </summary>
    public class BonusMeterProvider : BaseMeterProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMeterManager _meterManager;

        private readonly List<Tuple<string, MeterClassification>> _meters = new List<Tuple<string, MeterClassification>>
        {
            Tuple.Create<string, MeterClassification>(BonusMeters.MjtGamesPlayedCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(BonusMeters.HandPaidMjtBonusCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(BonusMeters.HandPaidMjtBonusAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(BonusMeters.HandPaidBonusCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(BonusMeters.EgmPaidMjtBonusCount, new OccurrenceMeterClassification()),
            Tuple.Create<string, MeterClassification>(BonusMeters.EgmPaidMjtBonusAmount, new CurrencyMeterClassification()),
            Tuple.Create<string, MeterClassification>(BonusMeters.EgmPaidBonusCount, new OccurrenceMeterClassification()),
        };

        public BonusMeterProvider(IPersistentStorageManager storage, IMeterManager meterManager)
            : base(@"Aristocrat.Monaco.Gaming.BonusMeterProvider")
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));

            RolloverTest = PropertiesManager.GetValue(@"maxmeters", "false") == "true";

            AddMeters(storage.GetAccessor(Level, Name));

            AddCompositeMeters();
        }

        private void AddMeters(IPersistentStorageAccessor accessor)
        {
            foreach (var meter in _meters)
            {
                var atomicMeter = new AtomicMeter(meter.Item1, accessor, meter.Item2, this);
                AddMeter(atomicMeter);
                SetupMeterRolloverTest(atomicMeter);
            }
        }

        private void AddCompositeMeters()
        {
            AddMeter(
                new CompositeMeter(
                    BonusMeters.BonusTotalCount,
                    (timeFrame) => _meterManager.GetMeter(BonusMeters.EgmPaidBonusCount).GetValue(timeFrame) +
                                   _meterManager.GetMeter(BonusMeters.HandPaidBonusCount).GetValue(timeFrame),
                    new List<string>
                    {
                        BonusMeters.EgmPaidBonusCount,
                        BonusMeters.HandPaidBonusCount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    BonusMeters.EgmPaidBonusGameWonAmount,
                    (timeFrame) => _meterManager.GetMeter(GamingMeters.EgmPaidBonusCashableInAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        GamingMeters.EgmPaidBonusCashableInAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    BonusMeters.EgmPaidBonusGameNonWonAmount,
                    (timeFrame) => _meterManager.GetMeter(GamingMeters.EgmPaidBonusNonCashInAmount).GetValue(timeFrame) +
                                   _meterManager.GetMeter(GamingMeters.EgmPaidBonusPromoInAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        GamingMeters.EgmPaidBonusNonCashInAmount,
                        GamingMeters.EgmPaidBonusPromoInAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    BonusMeters.HandPaidBonusGameNonWonAmount,
                    (timeFrame) => _meterManager.GetMeter(GamingMeters.HandPaidBonusNonCashInAmount).GetValue(timeFrame) +
                                   _meterManager.GetMeter(GamingMeters.HandPaidBonusPromoInAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        GamingMeters.HandPaidBonusNonCashInAmount,
                        GamingMeters.HandPaidBonusPromoInAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                    new CompositeMeter(
                        BonusMeters.BonusCashableInAmount,
                        (timeFrame) => _meterManager.GetMeter(GamingMeters.EgmPaidBonusCashableInAmount).GetValue(timeFrame) +
                                       _meterManager.GetMeter(GamingMeters.HandPaidBonusCashableInAmount).GetValue(timeFrame),
                        new List<string>
                        {
                            GamingMeters.EgmPaidBonusCashableInAmount,
                            GamingMeters.HandPaidBonusCashableInAmount
                        },
                        new CurrencyMeterClassification()));

            AddMeter(
                    new CompositeMeter(
                        BonusMeters.BonusNonCashableInAmount,
                        (timeFrame) => _meterManager.GetMeter(GamingMeters.EgmPaidBonusNonCashInAmount).GetValue(timeFrame) +
                                       _meterManager.GetMeter(GamingMeters.HandPaidBonusNonCashInAmount).GetValue(timeFrame),
                        new List<string>
                        {
                            GamingMeters.EgmPaidBonusNonCashInAmount,
                            GamingMeters.HandPaidBonusNonCashInAmount
                        },
                        new CurrencyMeterClassification()));


            AddMeter(
                    new CompositeMeter(
                        BonusMeters.BonusPromoInAmount,
                        (timeFrame) => _meterManager.GetMeter(GamingMeters.EgmPaidBonusPromoInAmount).GetValue(timeFrame) +
                                       _meterManager.GetMeter(GamingMeters.HandPaidBonusPromoInAmount).GetValue(timeFrame),
                        new List<string>
                        {
                            GamingMeters.EgmPaidBonusPromoInAmount,
                            GamingMeters.HandPaidBonusPromoInAmount
                        },
                        new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    BonusMeters.MjtBonusAmount,
                    (timeFrame) => GetMeter(BonusMeters.HandPaidMjtBonusAmount).GetValue(timeFrame) +
                                   GetMeter(BonusMeters.EgmPaidMjtBonusAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        BonusMeters.HandPaidMjtBonusAmount,
                        BonusMeters.EgmPaidMjtBonusAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    BonusMeters.MjtBonusCount,
                    (timeFrame) => GetMeter(BonusMeters.HandPaidMjtBonusCount).GetValue(timeFrame) +
                                   GetMeter(BonusMeters.EgmPaidMjtBonusCount).GetValue(timeFrame),
                    new List<string>
                    {
                        BonusMeters.HandPaidMjtBonusCount,
                        BonusMeters.EgmPaidMjtBonusCount
                    },
                    new OccurrenceMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    BonusMeters.BonusBaseCashableInAmount,
                    (timeFrame) => _meterManager.GetMeter(GamingMeters.EgmPaidBonusCashableInAmount).GetValue(timeFrame) -
                                   _meterManager.GetMeter(GamingMeters.EgmPaidBonusNonDeductibleAmount).GetValue(timeFrame) -
                                   _meterManager.GetMeter(GamingMeters.EgmPaidBonusDeductibleAmount).GetValue(timeFrame) -
                                   _meterManager.GetMeter(GamingMeters.EgmPaidBonusWagerMatchAmount).GetValue(timeFrame) -
                                   GetMeter(BonusMeters.EgmPaidMjtBonusAmount).GetValue(timeFrame),
                    new List<string>
                    {
                        GamingMeters.EgmPaidBonusCashableInAmount,
                        GamingMeters.EgmPaidBonusNonDeductibleAmount,
                        GamingMeters.EgmPaidBonusDeductibleAmount,
                        GamingMeters.EgmPaidBonusWagerMatchAmount,
                        BonusMeters.EgmPaidMjtBonusAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
            new CompositeMeter(
                BonusMeters.HandPaidBonusBaseCashableInAmount,
                (timeFrame) => _meterManager.GetMeter(GamingMeters.HandPaidBonusCashableInAmount).GetValue(timeFrame) -
                               _meterManager.GetMeter(GamingMeters.HandPaidBonusNonDeductibleAmount).GetValue(timeFrame) -
                               _meterManager.GetMeter(GamingMeters.HandPaidBonusDeductibleAmount).GetValue(timeFrame) -
                               _meterManager.GetMeter(GamingMeters.HandPaidBonusWagerMatchAmount).GetValue(timeFrame) -
                               GetMeter(BonusMeters.HandPaidMjtBonusAmount).GetValue(timeFrame),
                new List<string>
                {
                    GamingMeters.HandPaidBonusCashableInAmount,
                    GamingMeters.HandPaidBonusNonDeductibleAmount,
                    GamingMeters.HandPaidBonusDeductibleAmount,
                    GamingMeters.HandPaidBonusWagerMatchAmount,
                    BonusMeters.HandPaidMjtBonusAmount
                },
                new CurrencyMeterClassification()));
        }
    }
}