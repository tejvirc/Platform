namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Contracts.Tickets;
    using Hardware.Contracts.NoteAcceptor;
    using Monaco.Localization.Properties;

    public class MeterTextTicket : TextTicket
    {
        public const int Master = 0;
        public const int Period = 1;
        private const decimal MillicentDivisor = 100000L;
        private const string VoucherIn = "System.VoucherIn";
        private const string Count = "Count";
        private const string Value = "Value";

        protected static readonly string[] MeterInNames =
        {
            "VoucherInCashable", "VoucherInCashablePromotional", "VoucherInNonCashablePromotional"
        };

        protected static readonly Dictionary<string, string> CoinInMeterNames = new Dictionary<string, string>();

        public MeterTextTicket()
            : base(Localizer.For(CultureFor.OperatorTicket))
        {
            Title = string.Empty;

            if (!CoinInMeterNames.Any())
            {
                CoinInMeterNames.Add(
                    TicketLocalizer.FormatString(ResourceKeys.CoinInFormat) + " " + 1.FormattedCurrencyString("C0"),
                    "CoinCount1s");
                CoinInMeterNames.Add(
                    TicketLocalizer.FormatString(ResourceKeys.CoinInFormat) + " " + 2.FormattedCurrencyString("C0"),
                    "CoinCount2s");
            }
        }

        /// <summary>
        ///     A list of the Voucher Out meter names used to calculate Amount out
        /// </summary>
        protected static Dictionary<string, string> BillInMeterNames
        {
            get
            {
                var noteAcceptor = Kernel.ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                if (noteAcceptor != null)
                {
                    return noteAcceptor.Denominations.ToDictionary(
                        denom => Localizer.For(CultureFor.OperatorTicket).FormatString(ResourceKeys.BillInFormat) +
                                 " " + denom.FormattedCurrencyString("C0"),
                        denom => "BillCount" + denom + "s");
                }

                return new Dictionary<string, string>();
            }
        }

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

        public Tuple<string, long[]> FillInCurrencyInfo(
            int denomination,
            CurrencyType currencyType,
            long[] counts,
            long[] totals)
        {
            // add the denominations to the denominations stack panel as a currency type
            var label = string.Format(
                TicketLocalizer.CurrentCulture,
                "{0} {1:C0}",
                currencyType == CurrencyType.Bill
                    ? TicketLocalizer.GetString(ResourceKeys.BillEventsLogsWindowTitle)
                    : TicketLocalizer.GetString(ResourceKeys.CoinInText),
                denomination);

            // get the bill count for this denomination from the meter
            var meterManager = ServiceManager.GetService<IMeterManager>();

            // This is the assumption asserted above - that "BillCount" for counting bills in will have
            // a corresponding "CoinCount" meter to track inserted coins.
            var meterPrefix = currencyType == CurrencyType.Bill ? "BillCount" : "CoinCount";

            IMeter currencyMeter = null;
            var meterName = meterPrefix + denomination.ToString(CultureInfo.InvariantCulture) + "s";

            if (meterManager.IsMeterProvided(meterName))
            {
                currencyMeter = meterManager.GetMeter(meterName);
            }

            var periodCount = currencyMeter?.Period ?? 0L;
            var masterCount = currencyMeter?.Lifetime ?? 0L;

            var returnValue = new Tuple<string, long[]>(
                label,
                new[] { masterCount * denomination, periodCount * denomination });

            counts[Master] += masterCount;
            totals[Master] += returnValue.Item2[Master];

            counts[Period] += periodCount;
            totals[Period] += returnValue.Item2[Period];

            return returnValue;
        }

        protected static void PopulateValidMeters(
            IMeterManager meterManager,
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

                if (meterManager.IsMeterProvided(nameDict.Value))
                {
                    var meter = meterManager.GetMeter(nameDict.Value);
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
                        string.Format(NumberFormatInfo.CurrentInfo, "{0}", masterCount),
                        dict.Key,
                        string.Format(NumberFormatInfo.CurrentInfo, "{0}", periodCount));
                }
                else
                {
                    AddLine(masterV.FormattedCurrencyString(), dict.Key, periodV.FormattedCurrencyString());
                }
            }
        }

        protected void TicketPageContent(bool showCountLines)
        {
            long countMaster = 0;
            long countPeriod = 0;
            long totalBillInMaster = 0;
            long totalBillInPeriod = 0;

            var meterManager = ServiceManager.GetService<IMeterManager>();
            var moneyInLifetimeCountDictionary = new Dictionary<string, long>();
            var moneyInCountPeriodDictionary = new Dictionary<string, long>();

            PopulateValidMeters(
                meterManager,
                ref countMaster,
                CoinInMeterNames,
                moneyInLifetimeCountDictionary,
                moneyInCountPeriodDictionary,
                ref countPeriod);

            moneyInLifetimeCountDictionary.Clear();
            moneyInCountPeriodDictionary.Clear();
            countMaster = 0;
            countPeriod = 0;

            PopulateValidMeters(
                meterManager,
                ref countMaster,
                BillInMeterNames,
                moneyInLifetimeCountDictionary,
                moneyInCountPeriodDictionary,
                ref countPeriod);

            AddLine(
                string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", countMaster),
                TicketLocalizer.GetString(ResourceKeys.NumberOfBillsInserted),
                string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", countPeriod));

            /*Add txt to the left string which will be displayed on the left side of the ticket*/
            AddTicketText(
                moneyInLifetimeCountDictionary,
                moneyInCountPeriodDictionary,
                ref totalBillInMaster,
                ref totalBillInPeriod,
                showCountLines);

            AddLine(
                totalBillInMaster.FormattedCurrencyString(),
                TicketLocalizer.GetString(ResourceKeys.TotalDashBillIn),
                totalBillInPeriod.FormattedCurrencyString());

            AddDashesLine();

            VoucherData(meterManager);

            GameData(meterManager);
        }

        protected void VoucherData(IMeterManager meterManager)
        {
            var voucherInEnabled = (bool)PropertiesManager.GetProperty(VoucherIn, false);

            if (voucherInEnabled)
            {
                //voucher in 
                long totalVoucherInCountM = 0;
                long totalVoucherInCountP = 0;
                double totalVoucherInValueM = 0;
                double totalVoucherInValueP = 0;

                foreach (var name in MeterInNames)
                {
                    var meter = meterManager.GetMeter(name + Count);
                    totalVoucherInCountM += meter.Lifetime;
                    totalVoucherInCountP += meter.Period;

                    var value = meterManager.GetMeter(name + Value);
                    totalVoucherInValueM += value.Lifetime;
                    totalVoucherInValueP += value.Period;
                }

                AddLine(
                    string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", totalVoucherInCountM),
                    TicketLocalizer.GetString(ResourceKeys.VoucherInTickets),
                    string.Format(NumberFormatInfo.CurrentInfo, "{0:N0}", totalVoucherInCountP));

                var multiplier = 1.0 / (double)PropertiesManager.GetProperty(
                    ApplicationConstants.CurrencyMultiplierKey,
                    null);
                AddLine(
                    (totalVoucherInValueM * multiplier).FormattedCurrencyString(),
                    TicketLocalizer.GetString(ResourceKeys.VoucherInAmount),
                    (totalVoucherInValueP * multiplier).FormattedCurrencyString());

                AddDashesLine();
            }
        }

        protected void GameData(IMeterManager meterManager)
        {
            //games played/won
            var playedCount = meterManager.IsMeterProvided("PlayedCount") ? meterManager.GetMeter("PlayedCount") : null;
            var masterPowerOffPlayedCount = GetAssociatedPowerOffMeterValue(meterManager, "PlayedCount", false);
            var periodPowerOffPlayedCount = GetAssociatedPowerOffMeterValue(meterManager, "PlayedCount", true);
            AddLine(
                playedCount?.Classification.CreateValueString(playedCount.Lifetime + masterPowerOffPlayedCount) ?? "0",
                TicketLocalizer.GetString(ResourceKeys.GamesPlayed),
                playedCount?.Classification.CreateValueString(playedCount.Period + periodPowerOffPlayedCount) ?? "0");

            var wonCount = meterManager.IsMeterProvided("WonCount") ? meterManager.GetMeter("WonCount") : null;
            var masterPowerOffWonCount = GetAssociatedPowerOffMeterValue(meterManager, "WonCount", false);
            var periodPowerOffWonCount = GetAssociatedPowerOffMeterValue(meterManager, "WonCount", true);
            AddLine(
                wonCount?.Classification.CreateValueString(wonCount.Lifetime + masterPowerOffWonCount) ?? "0",
                TicketLocalizer.GetString(ResourceKeys.GamesWon),
                wonCount?.Classification.CreateValueString(wonCount.Period + periodPowerOffWonCount) ?? "0");

            AddDashesLine();
        }
    }
}