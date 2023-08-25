namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.Tickets;
    using Contracts;
    using Contracts.Bonus;
    using Localization.Properties;

    /// <summary>
    /// </summary>
    public class GameBonusInfoTicket : TextTicket
    {
        private readonly string _bonusInfo;
        private readonly IEnumerable<BonusInfoMeter> _items;

        public GameBonusInfoTicket(
            string bonusInfo,
            IEnumerable<BonusInfoMeter> items)
            : base(Localizer.For(CultureFor.PlayerTicket))
        {
            _items = items;
            _bonusInfo = bonusInfo;

            Title = TicketLocalizer.GetString(ResourceKeys.GameBonusInfoTicketTitleText);
        }

        public override void AddTicketContent()
        {
            if (_items == null || !_items.Any())
            {
                return;
            }

            AddLine(null, Dashes, null);
            AddLine(null, _bonusInfo, null);

            AddLine(
                TicketLocalizer.GetString(ResourceKeys.NameLabel),
                "",
                TicketLocalizer.GetString(ResourceKeys.Value));

            foreach (var item in _items)
            {
                var value = FormatCurrency(item.MeterValue);
                AddLine(
                    item.MeterName,
                    null,
                    value);
            }

            AddLine(null, Dashes, null);

            AddTicketFooter();
        }

        private static string FormatCurrency(long value)
        {
            var dec = Convert.ToDecimal(value / (100.00 * GamingConstants.Millicents));
            var val = dec.FormattedCurrencyString();
            return val.PadLeft(val.Length == 0 ? 10 : 9);
        }
    }
}