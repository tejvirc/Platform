namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;

    public class PeriodicResetTicket : MeterTextTicket
    {
        private long[] _billCountIn;
        private long[] _billTotalIn;
        private long[] _coinCountIn;
        private long[] _coinTotalIn;

        public PeriodicResetTicket()
        {
            Title = TicketLocalizer.GetString(ResourceKeys.PeriodicResetTicketTitle);
        }

        public override void AddTicketContent()
        {
            var meterManager = ServiceManager.GetService<IMeterManager>();
            AddLine(TicketLocalizer.GetString(ResourceKeys.MasterText), " ", TicketLocalizer.GetString(ResourceKeys.PeriodText));
            AddDashesLine();
            AddLine(
                Time.GetLocationTime(meterManager.LastMasterClear).ToString(DateTimeFormat),
                null,
                Time.GetLocationTime(meterManager.LastPeriodClear).ToString(DateTimeFormat));

            var config = ServiceManager.GetService<IOperatorMenuConfiguration>();

            var showCoinData = config.GetSetting(OperatorMenuSetting.MainMetersPageViewModel, OperatorMenuSetting.CoinsDataVisibility, false);
            var showBillDataInCount = config.GetSetting(OperatorMenuSetting.MainMetersPageViewModel, OperatorMenuSetting.BillDataInCountState, true);

            if (!showCoinData)
            {
                TicketPageContent(showBillDataInCount);

                SetAmounts(meterManager);

                AddDashesLine();

                SetBalance();

                AddTicketFooter();

                return;
            }

            // fill in the data for each bill
            _coinCountIn = new[] { 0L, 0L };
            _coinTotalIn = new[] { 0L, 0L };
            _billCountIn = new[] { 0L, 0L };
            _billTotalIn = new[] { 0L, 0L };

            var coinLines = new List<Tuple<string, long[]>>();
            foreach (
                var i in (Collection<int>)PropertiesManager.GetProperty(
                    "CoinDenominations",
                    new Collection<int> { 1, 2 }))
            {
                coinLines.Add(FillInCurrencyInfo(i, CurrencyType.Coin, _coinCountIn, _coinTotalIn));
            }

            var billLines = new List<Tuple<string, long[]>>();
            foreach (var i in ServiceManager.TryGetService<INoteAcceptor>()?.Denominations ?? new List<int>())
            {
                billLines.Add(FillInCurrencyInfo(i, CurrencyType.Bill, _billCountIn, _billTotalIn));
            }

            // AddLine(null, $"{GetDashString(26)} {TicketLocalizer.GetString("CoinInText} {GetDashString(26)}", null);
            AddMeterLine(TicketLocalizer.GetString(ResourceKeys.NumCoinsInText), _coinCountIn, "{0}");
            foreach (var tuple in coinLines)
            {
                AddMeterLine(tuple.Item1, tuple.Item2);
            }

            AddMeterLine(TicketLocalizer.GetString(ResourceKeys.TotalCoinInText), _coinTotalIn);

            //AddLine(null, $"{GetDashString(26)} {TicketLocalizer.GetString("BillInText} {GetDashString(26)}", null);
            AddMeterLine(TicketLocalizer.GetString(ResourceKeys.NumBillsInText), _billCountIn, "{0}");
            foreach (var tuple in billLines)
            {
                AddMeterLine(tuple.Item1, tuple.Item2);
            }

            AddMeterLine(TicketLocalizer.GetString(ResourceKeys.TotalBillsInText), _billTotalIn);

            AddLine(null, TicketLocalizer.GetString(ResourceKeys.TotalCoinsAndBillsText), null);
            var amountIn = _coinTotalIn.Zip(_billTotalIn, (c, b) => c + b).ToArray();
            AddMeterLine(string.Empty, amountIn);

            VoucherData(meterManager);

            SetAmounts(meterManager);

            AddDashesLine();

            SetBalance();

            AddTicketFooter();
        }

        protected void SetAmounts(IMeterManager meterManager)
        {
            var totalInMeter = meterManager.GetMeter(ApplicationMeters.TotalIn);
            var totalIn = new[] { FromMillicents(totalInMeter.Lifetime), FromMillicents(totalInMeter.Period) };
            AddMeterLine(TicketLocalizer.GetString(ResourceKeys.AmountInText), totalIn);

            var totalOutMeter = meterManager.GetMeter(ApplicationMeters.TotalOut);
            var totalOut = new[] { FromMillicents(totalOutMeter.Lifetime), FromMillicents(totalOutMeter.Period) };
            AddMeterLine(TicketLocalizer.GetString(ResourceKeys.AmountOutText), totalOut);

            var net = totalIn.Zip(totalOut, (ins, outs) => ins - outs).ToArray();
            AddMeterLine(TicketLocalizer.GetString(ResourceKeys.NetAmountText), net);
        }

        private void SetBalance()
        {
            var balance = (long)PropertiesManager.GetProperty(PropertyKey.CurrentBalance, 0L);
            AddLine(
                TicketLocalizer.GetString(ResourceKeys.CreditBalanceText),
                null,
                FromMillicents(balance).FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture));
        }

        private void AddMeterLine(string labelText, long[] meterPair, string meterFormatString = "")
        {
            if (string.IsNullOrEmpty(meterFormatString))
            {
                AddLine(
                    meterPair[Master].FormattedCurrencyString(),
                labelText,
                    meterPair[Period].FormattedCurrencyString());
            }
            else
            {
                AddLine(
                    string.Format(CultureInfo.CurrentCulture, meterFormatString, meterPair[Master]),
                    labelText,
                    string.Format(CultureInfo.CurrentCulture, meterFormatString, meterPair[Period]));
            }
        }

        private void AddMeterLine(string labelText, decimal[] meterPair, string meterFormatString = "")
        {
            if (string.IsNullOrEmpty(meterFormatString))
            {
                AddLine(
                    meterPair[Master].FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture),
                    labelText,
                    meterPair[Period].FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture));
            }
            else
            {
                AddLine(
                    string.Format(CultureInfo.CurrentCulture, meterFormatString, meterPair[Master]),
                    labelText,
                    string.Format(CultureInfo.CurrentCulture, meterFormatString, meterPair[Period]));
            }
        }
    }
}
