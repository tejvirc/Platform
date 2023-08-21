namespace Aristocrat.Monaco.Gaming.VideoLottery.Tickets
{
    using System;
    using System.Text;
    using Application.Contracts.Localization;
    using Application.Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Localization.Properties;

    public class HelplineTicket : TextTicket
    {
        public HelplineTicket()
            : base(Localizer.For(CultureFor.PlayerTicket))
        {
            var title = new StringBuilder(TicketLocalizer.GetString(ResourceKeys.HelplineTicketTitle));
            for (var i = 0; i < 8; i++)
            {
                title.AppendLine();
            }

            title.Append(TicketLocalizer.GetString(ResourceKeys.HelplineTicketTitleFr).Replace("\\r\\n", Environment.NewLine));
            Title = title.ToString();
        }

        public override Ticket CreateTextTicket()
        {
            AddTicketHeader();
            AddTicketContent();

            var ticket = CreateTicket(Title.ToUpper());
            return ticket;
        }

        /// <inheritdoc />
        public override int TicketCasinoInfoLineCount => 0;

        /// <inheritdoc />
        public override int TicketHeaderLineCount => 1;

        public override void AddTicketHeader()
        {
            AddLine(null, null, null);
        }

        public override void AddTicketContent()
        {
            AddLine(null, TicketLocalizer.GetString(ResourceKeys.HelpLineMessage).Replace("\\r\\n", Environment.NewLine), null);

            AddLine(null, null, null);
            AddLine(null, Dashes, null);
            AddLine(null, null, null);
            AddLine(null, null, null);
            AddLine(null, null, null);
            AddLine(null, TicketLocalizer.GetString(ResourceKeys.HelpLineMessageFr).Replace("\\r\\n", Environment.NewLine), null);
        }
    }
}
