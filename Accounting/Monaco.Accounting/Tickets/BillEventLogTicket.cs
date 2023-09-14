namespace Aristocrat.Monaco.Accounting.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.Tickets;
    using Contracts;
    using Kernel;
    using Localization.Properties;

    public class BillEventLogTicket : TextTicket
    {
        private readonly ICollection<BillTransaction> _events;

        private readonly int _page;

        public BillEventLogTicket(ICollection<BillTransaction> transactions, int eventsPerPage, int currentPage)
            : base(Localizer.For(CultureFor.PlayerTicket))
        {
            _events = transactions;
            ItemsPerPage = eventsPerPage;
            _page = currentPage;

            Title = TicketLocalizer.GetString(ResourceKeys.BillEventLogTicket);
        }

        public override void AddTicketContent()
        {
            if (_events == null || _events.Count == 0)
            {
                return;
            }

            int endIndex;
            int startIndex;

            checked
            {
                startIndex = ItemsPerPage * (_page - 1);
                endIndex = Math.Min(startIndex + ItemsPerPage, _events.Count) - 1;
            }

            AddLine(null, Dashes, null);
            for (var i = startIndex; i <= endIndex; i++)
            {
                var entry = _events.ElementAt(i);

                var multiplier = ServiceManager.GetService<IPropertiesManager>()
                    .GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);

                // NOTE: If additional lines are added here, update the BillEventLogTicketCreator.ItemLineLength
                AddLine(
                    TicketLocalizer.GetString(ResourceKeys.Date),
                    null,
                    Time.GetLocationTime(entry.Accepted).ToString(DateFormat));
                AddLine(
                    TicketLocalizer.GetString(ResourceKeys.TimeLabel),
                    null,
                    Time.GetLocationTime(entry.Accepted).ToString(TimeFormat));
                AddLine(
                    TicketLocalizer.GetString(ResourceKeys.Amount),
                    null,
                    $"{(entry.Amount / multiplier).FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture)}");
                AddLine(null, Dashes, null);
            }

            if (endIndex == _events.Count - 1)
            {
                AddTicketFooter();
            }
        }
    }
}