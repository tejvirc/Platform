namespace Aristocrat.Monaco.Accounting.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.Localization;
    using Application.Contracts.Tickets;
    using Contracts.Models;
    using Localization.Properties;

    /// <summary>
    /// </summary>
    public class HandpayTicket : TextTicket
    {
        private readonly ICollection<HandpayData> _items;

        public HandpayTicket(IList<HandpayData> items, int eventsPerPage, int currentPage)
            : base(Localizer.For(CultureFor.Player))
        {
            _items = items;
            ItemsPerPage = eventsPerPage;
            Page = currentPage;

            Title = TicketLocalizer.GetString(ResourceKeys.HandpayLogTicketTitle);
        }

        public int Page { get; set; }

        public override void AddTicketContent()
        {
            if (_items == null || _items.Count == 0)
            {
                return;
            }

            int endIndex;
            int startIndex;

            checked
            {
                startIndex = ItemsPerPage * (Page - 1);
                endIndex = Math.Min(startIndex + ItemsPerPage, _items.Count) - 1;
            }

            AddLine(null, Dashes, null);

            for (var i = startIndex; i <= endIndex; i++)
            {
                var entry = _items.ElementAt(i);
                if (entry == null)
                {
                    continue;
                }

                // NOTE: If additional lines are added here, update the HandpayTicketCreator.ItemLineLength
                AddLine(TicketLocalizer.GetString(ResourceKeys.HandpayTicketHandpayType), entry.HandpayType, null);
                var splitString = Time.GetLocationTime(DateTime.Parse(entry.TimeStamp)).ToString(DateAndTimeFormat).Split(' ');
                AddLine(TicketLocalizer.GetString(ResourceKeys.Date), null, splitString[0]);
                AddLine(TicketLocalizer.GetString(ResourceKeys.TimeLabel), null, splitString[1]);


                if (!string.IsNullOrEmpty(entry.CashableAmount))
                {
                    AddLine(TicketLocalizer.GetString(ResourceKeys.HandpayTicketCashableAmount), null, entry.CashableAmount);
                }

                if (!string.IsNullOrEmpty(entry.NonCashAmount))
                {
                    AddLine(TicketLocalizer.GetString(ResourceKeys.HandpayTicketNonCashableAmount), null, entry.NonCashAmount);
                }

                if (!string.IsNullOrEmpty(entry.PromoAmount))
                {
                    AddLine(TicketLocalizer.GetString(ResourceKeys.HandpayTicketPromoAmount), null, entry.PromoAmount);
                }

                if (!string.IsNullOrEmpty(entry.State))
                {
                    AddLine(TicketLocalizer.GetString(ResourceKeys.State), null, entry.State);
                }

                AddLine(null, Dashes, null);

            }

            if (endIndex == _items.Count - 1)
                AddTicketFooter();

        }

        /// <summary>
        /// </summary>
        public override void AddTicketHeader()
        {
            if (ConfigWizardUtil.VisibleByConfig(PropertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride))
            {
                AddLine(
                    $"{TicketLocalizer.GetString(ResourceKeys.RetailerNumber)}:", null,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0}",
                        PropertiesManager.GetProperty(ApplicationConstants.Zone, TicketLocalizer.GetString(ResourceKeys.DataUnavailable))));
            }

            var now = ServiceManager.GetService<ITime>().GetLocationTime(DateTime.UtcNow);
            AddLine(
                $"{TicketLocalizer.GetString(ResourceKeys.Date)}: {now.ToString(DateFormat, CultureInfo.CurrentCulture)}",
                null,
                $"{TicketLocalizer.GetString(ResourceKeys.TimeLabel)}: {now.ToString(TimeFormat, CultureInfo.CurrentCulture)}");

            AddTicketHeaderCommonPart();
        }



    }
}
