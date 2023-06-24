namespace Aristocrat.Monaco.Application.Tickets
{
    using System.Linq;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Ticket;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;

    public class PeriodicResetTicket : MeterTextTicket
    {

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

            var showCoinData = OperatorMenuConfig?.GetSetting(OperatorMenuSetting.MainMetersPageViewModel, OperatorMenuSetting.CoinsDataVisibility, false) ?? false;
            var showBillDataInCount = OperatorMenuConfig?.GetSetting(OperatorMenuSetting.MainMetersPageViewModel, OperatorMenuSetting.BillDataInCountState, true) ?? true;

            TicketPageContent(showBillDataInCount, false, showCoinData);

            SetAmounts();

            AddDashesLine();

            SetBalance();

            AddTicketFooter();
        }

        public override Ticket CreateSecondPageTextTicket()
        {
            AddCasinoInfo();
            AddTicketHeader();
            AddLine(TicketLocalizer.GetString(ResourceKeys.MasterText), " ", TicketLocalizer.GetString(ResourceKeys.PeriodText));
            AddDashesLine();
            AddLine(
                Time.GetLocationTime(MeterManager.LastMasterClear).ToString(DateTimeFormat),
                null,
                Time.GetLocationTime(MeterManager.LastPeriodClear).ToString(DateTimeFormat));

            return CreateTicket(Title);
        }

        protected void SetAmounts()
        {
            var totalInMeter = MeterManager.GetMeter(ApplicationMeters.TotalIn);
            var totalIn = new[] { FromMillicents(totalInMeter.Lifetime), FromMillicents(totalInMeter.Period) };
            AddMeterLine(TicketLocalizer.GetString(ResourceKeys.AmountInText), totalIn);

            var totalOutMeter = MeterManager.GetMeter(ApplicationMeters.TotalOut);
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
                    string.Format(TicketLocalizer.CurrentCulture, meterFormatString, meterPair[Master]),
                    labelText,
                    string.Format(TicketLocalizer.CurrentCulture, meterFormatString, meterPair[Period]));
            }
        }
    }
}
