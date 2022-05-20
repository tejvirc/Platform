namespace Aristocrat.Monaco.Accounting.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.Tickets;
    using Contracts.Tickets;
    using Contracts.Vouchers;
    using Localization.Properties;

    public class CashSlipEventLogTicket : TextTicket
    {
        private readonly ICollection<CashSlipEventLogRecord> _events;
        private readonly int _page;

        public CashSlipEventLogTicket(IList<CashSlipEventLogRecord> events, int eventsPerPage, int currentPage)
            : base(Localizer.For(CultureFor.Player))
        {
            _events = events;
            ItemsPerPage = eventsPerPage;
            _page = currentPage;

            Title = TicketLocalizer.GetString(ResourceKeys.CashSlipEventLogTicket);
        }

        public override void AddTicketContent()
        {
            if (_events == null || !_events.Any())
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
                // NOTE: If additional lines are added here, update the CashSlipEventLogTicketCreator.ItemLineLength
                var entry = _events.ElementAt(i);
                AddLine(TicketLocalizer.GetString(ResourceKeys.DateTime), null, entry.TimeStamp);
                AddLine(TicketLocalizer.GetString(ResourceKeys.Amount), null, $"{entry.Amount.FormattedCurrencyString()}");

                var ticketNumber = string.Empty; //Resources.StatusError;
                if (!string.IsNullOrEmpty(entry.Barcode))
                {
                    ticketNumber = entry.Barcode.Length >= 4 ? VoucherExtensions.GetValidationString(entry.Barcode) : entry.Barcode;
                }

                AddLine(TicketLocalizer.GetString(ResourceKeys.TicketNumber), null, $"{ticketNumber}");
                AddLine(null, Dashes, null);
            }

            if (endIndex == _events.Count - 1)
            {
                AddTicketFooter();
            }
        }
    }
}