namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Monaco.Localization.Properties;

    public class MeterTextTicket : TextTicket
    {
        public const int Master = 0;
        public const int Period = 1;

        private const decimal MillicentDivisor = 100000L;
        private const string VoucherIn = "System.VoucherIn";
        private const string Count = "Count";
        private const string Value = "Value";
        private IOperatorMenuConfiguration _operatorMenuConfig;
        private IMeterManager _meterManager;
        private readonly bool _voucherInEnabled;

        protected const string CoinDenominationsPropertyKey = "CoinDenominations";
        protected static Collection<int> CoinDenominations = new Collection<int>();

        protected static readonly string[] MeterInNames =
        {
            "VoucherInCashable", "VoucherInCashablePromotional", "VoucherInNonCashablePromotional"
        };

        protected static readonly Dictionary<string, string> CoinInMeterNames = new Dictionary<string, string>();

        public MeterTextTicket()
            : base(Localizer.For(CultureFor.OperatorTicket))
        {
            Title = string.Empty;
            if (!CoinDenominations.Any())
            {
                CoinDenominations = PropertiesManager.GetValue(CoinDenominationsPropertyKey, new Collection<int> { 1, 2 });
            }


            // Need to reset these meter names if the language changed
            if (!CoinInMeterNames.Any(pair => pair.Key.Contains(TicketLocalizer.GetString(ResourceKeys.CoinInFormat))))
            {
                CoinInMeterNames.Clear();
                foreach (var denomination in CoinDenominations)
                {
                    CoinInMeterNames.Add(
                        $"{TicketLocalizer.FormatString(ResourceKeys.CoinInFormat)} {denomination.FormattedCurrencyString("C0")}",
                        $"CoinCount{denomination}s"
                        );
                }
            }

            _voucherInEnabled = PropertiesManager?.GetValue(VoucherIn, false) ?? false;
        }

        /// <summary>
        ///     A list of the Voucher Out meter names used to calculate Amount out
        /// </summary>
        protected static Dictionary<string, string> BillInMeterNames
        {
            get
            {
                var noteAcceptor = Kernel.ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                var properties = Kernel.ServiceManager.GetInstance().TryGetService<Kernel.IPropertiesManager>();

                // Can't use TicketLocalizer here due to static property
                var localizer =
                    properties?.GetValue(
                        ApplicationConstants.LocalizationOperatorTicketLanguageSettingOperatorOverride,
                        false) ?? false
                        ? Localizer.For(CultureFor.Operator)
                        : Localizer.For(CultureFor.OperatorTicket) ?? Localizer.For(CultureFor.OperatorTicket);

                if (noteAcceptor != null)
                {
                    return noteAcceptor.Denominations.ToDictionary(
                        denom => localizer.FormatString(ResourceKeys.BillInFormat) +
                                 " " + denom.FormattedCurrencyString("C0", localizer.CurrentCulture),
                        denom => "BillCount" + denom + "s");
                }

                return new Dictionary<string, string>();
            }
        }

        protected IOperatorMenuConfiguration OperatorMenuConfig => _operatorMenuConfig ??= ServiceManager.TryGetService<IOperatorMenuConfiguration>();

        protected IMeterManager MeterManager => _meterManager ??= ServiceManager.TryGetService<IMeterManager>();

        /// <summary>
        ///     Gets the interface this service implements and exposes as a service
        /// </summary>

        public string DateTimeFormat => $"{DateFormat} {TimeFormat}";

        public override void AddTicketContent()
        {
        }

        /// <summary>
        ///     Convert millicents to decimal
        /// </summary>
        /// <param name="value">The millicents value</param>
        /// <returns>The decimal value</returns>
        public decimal FromMillicents(long value)
        {
            return value / MillicentDivisor;
        }

        protected void PopulateValidMeters(
            ref long masterCount,
            Dictionary<string, string> meterNames,
            IDictionary<string, long> lifetimeCountDictionary,
            IDictionary<string, long> periodCountDictionary,
            ref long periodCount)
        {
            foreach (var nameDict in meterNames)
            {
                long l = 0;
                long p = 0;

                if (MeterManager.IsMeterProvided(nameDict.Value))
                {
                    var meter = MeterManager.GetMeter(nameDict.Value);
                    l = meter.Lifetime;
                    p = meter.Period;
                    masterCount += meter.Lifetime;
                    periodCount += meter.Period;
                }

                lifetimeCountDictionary.Add(nameDict.Key, l);
                periodCountDictionary.Add(nameDict.Key, p);
            }
        }

        protected static long GetAssociatedPowerOffMeterValue(
            IMeterManager meterManager,
            string meterName,
            bool isPeriodMeter)
        {
            var associatedPowerOffMeterName = string.Empty;

            switch (meterName)
            {
                case ApplicationMeters.MainDoorOpenCount:
                    associatedPowerOffMeterName = ApplicationMeters.MainDoorOpenPowerOffCount;
                    break;
                case ApplicationMeters.CashDoorOpenCount:
                    associatedPowerOffMeterName = ApplicationMeters.CashDoorOpenPowerOffCount;
                    break;
                case ApplicationMeters.LogicDoorOpenCount:
                    associatedPowerOffMeterName = ApplicationMeters.LogicDoorOpenPowerOffCount;
                    break;
                case ApplicationMeters.TopBoxDoorOpenCount:
                    associatedPowerOffMeterName = ApplicationMeters.TopBoxDoorOpenPowerOffCount;
                    break;
                case ApplicationMeters.UniversalInterfaceBoxDoorOpenCount:
                    associatedPowerOffMeterName = ApplicationMeters.UniversalInterfaceBoxDoorOpenPowerOffCount;
                    break;
            }

            if (string.IsNullOrEmpty(associatedPowerOffMeterName))
            {
                return 0L;
            }

            var meter = meterManager.GetMeter(associatedPowerOffMeterName);

            if (isPeriodMeter)
            {
                return meter.Period;
            }

            return meter.Lifetime;
        }

        protected void AddTicketText(
            Dictionary<string, long> masterMetersDictionary,
            Dictionary<string, long> periodMetersDictionary,
            ref long masterMeter,
            ref long periodMeter,
            bool countLine)
        {
            foreach (var dict in masterMetersDictionary)
            {
                var result = Regex.Replace(dict.Key, @"[^\d]", string.Empty); /* extract only numbers*/
                var masterV = dict.Value;
                var masterCount = masterV;
                periodMetersDictionary.TryGetValue(dict.Key, out var periodV);
                var periodCount = periodV;

                masterV *= Convert.ToInt32(result);
                periodV *= Convert.ToInt32(result);
                masterMeter += masterV;
                periodMeter += periodV;

                if (countLine)
                {
                    AddLine(
                        string.Format(TicketLocalizer.CurrentCulture.NumberFormat, "{0}", masterCount),
                        dict.Key,
                        string.Format(TicketLocalizer.CurrentCulture.NumberFormat, "{0}", periodCount));
                }
                else
                {
                    AddLine(
                        masterV.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture),
                        dict.Key,
                        periodV.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture));
                }
            }
        }


        protected void TicketPageContent(bool useCountLines, bool showGameData, bool showCoinData = false)
        {
            var countMaster = 0L;
            var countPeriod = 0L;
            var totalCoinInMaster = 0L;
            var totalCoinInPeriod = 0L;
            var totalBillInMaster = 0L;
            var totalBillInPeriod = 0L;
            var moneyInLifetimeCountDictionary = new Dictionary<string, long>();
            var moneyInCountPeriodDictionary = new Dictionary<string, long>();

            if (showCoinData)
            {
                TicketPageCoinContent();
            }

            moneyInLifetimeCountDictionary.Clear();
            moneyInCountPeriodDictionary.Clear();
            countMaster = 0;
            countPeriod = 0;

            TicketPageBillContent();

            if (showCoinData)
            {

                var sumCoinsAndBillsMaster = totalBillInMaster + totalCoinInMaster;
                var sumCoinsAndBillsPeriod = totalBillInPeriod + totalCoinInPeriod;

                AddLine(
                    sumCoinsAndBillsMaster.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture),
                    TicketLocalizer.GetString(ResourceKeys.TotalDashCoinsAndBills),
                    sumCoinsAndBillsPeriod.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture));
            }

            if (_voucherInEnabled)
            {
                VoucherData();

                var (_, _, ValueMaster, ValuePeriod) = GetVoucherInMeterData();
                var valueSumMaster = ValueMaster + totalBillInMaster + totalCoinInMaster;
                var valueSumPeriod = ValuePeriod + totalBillInPeriod + totalCoinInPeriod;

                var localizedFieldName = TicketLocalizer.GetString(
                    showCoinData ?
                    ResourceKeys.TotalDashCoinsBillsAndVouchers :
                    ResourceKeys.TotalDashBillsAndVouchers);

                AddLine(
                    valueSumMaster.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture),
                    localizedFieldName,
                    valueSumPeriod.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture));
            }

            AddDashesLine(); // Add dashes here to group money-in fields together

            if (showGameData)
            {
                GameData();
            }

            void TicketPageCoinContent()
            {    


                PopulateValidMeters(
                    ref countMaster,
                    CoinInMeterNames,
                    moneyInLifetimeCountDictionary,
                    moneyInCountPeriodDictionary,
                    ref countPeriod);

                AddLine(
                    string.Format(TicketLocalizer.CurrentCulture.NumberFormat, "{0:N0}", countMaster),
                    TicketLocalizer.GetString(ResourceKeys.NumCoinsInText),
                    string.Format(TicketLocalizer.CurrentCulture.NumberFormat, "{0:N0}", countPeriod));

                AddTicketText(
                    moneyInLifetimeCountDictionary,
                    moneyInCountPeriodDictionary,
                    ref totalCoinInMaster,
                    ref totalCoinInPeriod,
                    useCountLines);

                AddLine(
                    totalCoinInMaster.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture),
                    TicketLocalizer.GetString(ResourceKeys.TotalDashCoinIn),
                    totalCoinInPeriod.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture));
            }

            void TicketPageBillContent()
            {

                PopulateValidMeters(
                    ref countMaster,
                    BillInMeterNames,
                    moneyInLifetimeCountDictionary,
                    moneyInCountPeriodDictionary,
                    ref countPeriod);

                AddLine(
                    string.Format(TicketLocalizer.CurrentCulture.NumberFormat, "{0:N0}", countMaster),
                    TicketLocalizer.GetString(ResourceKeys.NumberOfBillsInserted),
                    string.Format(TicketLocalizer.CurrentCulture.NumberFormat, "{0:N0}", countPeriod));

                /*Add txt to the left string which will be displayed on the left side of the ticket*/
                AddTicketText(
                    moneyInLifetimeCountDictionary,
                    moneyInCountPeriodDictionary,
                    ref totalBillInMaster,
                    ref totalBillInPeriod,
                    useCountLines);

                AddLine(
                    totalBillInMaster.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture),
                    TicketLocalizer.GetString(ResourceKeys.TotalDashBillIn),
                    totalBillInPeriod.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture));
            }
        }

        protected void VoucherData()
        {
            //voucher in
            if (_voucherInEnabled)
            {
                var (CountMaster, CountPeriod, ValueMaster, ValuePeriod) = GetVoucherInMeterData();

                AddLine(
                    string.Format(TicketLocalizer.CurrentCulture.NumberFormat, "{0:N0}", CountMaster),
                    TicketLocalizer.GetString(ResourceKeys.VoucherInTickets),
                    string.Format(TicketLocalizer.CurrentCulture.NumberFormat, "{0:N0}", CountPeriod));

                AddLine(
                    ValueMaster.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture),
                    TicketLocalizer.GetString(ResourceKeys.VoucherInAmount),
                    ValuePeriod.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture));
            }

        }

        protected void GameData()
        {
            //games played/won
            var playedCount = MeterManager.IsMeterProvided("PlayedCount") ? MeterManager.GetMeter("PlayedCount") : null;
            var masterPowerOffPlayedCount = GetAssociatedPowerOffMeterValue(MeterManager, "PlayedCount", false);
            var periodPowerOffPlayedCount = GetAssociatedPowerOffMeterValue(MeterManager, "PlayedCount", true);
            AddLine(
                playedCount?.Classification.CreateValueString(playedCount.Lifetime + masterPowerOffPlayedCount) ?? "0",
                TicketLocalizer.GetString(ResourceKeys.GamesPlayed),
                playedCount?.Classification.CreateValueString(playedCount.Period + periodPowerOffPlayedCount) ?? "0");

            var wonCount = MeterManager.IsMeterProvided("WonCount") ? MeterManager.GetMeter("WonCount") : null;
            var masterPowerOffWonCount = GetAssociatedPowerOffMeterValue(MeterManager, "WonCount", false);
            var periodPowerOffWonCount = GetAssociatedPowerOffMeterValue(MeterManager, "WonCount", true);
            AddLine(
                wonCount?.Classification.CreateValueString(wonCount.Lifetime + masterPowerOffWonCount) ?? "0",
                TicketLocalizer.GetString(ResourceKeys.GamesWon),
                wonCount?.Classification.CreateValueString(wonCount.Period + periodPowerOffWonCount) ?? "0");
        }

        protected (long CountMaster, long CountPeriod, double ValueMaster, double ValuePeriod) GetVoucherInMeterData()
        {
            long totalVoucherInCountM = 0;
            long totalVoucherInCountP = 0;
            double totalVoucherInValueM = 0;
            double totalVoucherInValueP = 0;
            var multiplier = 1.0 / (double)PropertiesManager.GetProperty(
                ApplicationConstants.CurrencyMultiplierKey,
                ApplicationConstants.DefaultCurrencyMultiplier);

            foreach (var name in MeterInNames)
            {
                var meter = MeterManager.GetMeter(name + Count);
                totalVoucherInCountM += meter.Lifetime;
                totalVoucherInCountP += meter.Period;

                var value = MeterManager.GetMeter(name + Value);
                totalVoucherInValueM += value.Lifetime;
                totalVoucherInValueP += value.Period;
            }

            totalVoucherInValueM *= multiplier;
            totalVoucherInValueP *= multiplier;

            return (totalVoucherInCountM, totalVoucherInCountP, totalVoucherInValueM, totalVoucherInValueP);
        }
    }
}