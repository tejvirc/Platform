namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.Tickets;
    using Contracts;
    using Contracts.Progressives;
    using Localization.Properties;

    /// <summary>
    /// </summary>
    public class GameBonusInfoTicket : TextTicket
    {
        private readonly string _bonusInfo;
        private readonly string _denomination;
        private readonly IEnumerable<IViewableProgressiveLevel> _items;

        public GameBonusInfoTicket(
            string bonusInfo,
            string denomination,
            IEnumerable<IViewableProgressiveLevel> items)
            : base(Localizer.For(CultureFor.PlayerTicket))
        {
            _items = items;
            _bonusInfo = bonusInfo;
            _denomination = denomination;

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
                TicketLocalizer.GetString(ResourceKeys.DenominationText),
                _denomination,
                null);

            AddLine(
                TicketLocalizer.GetString(ResourceKeys.NameLabel),
                $"   {TicketLocalizer.GetString(ResourceKeys.ValueResidualText)}",
                TicketLocalizer.GetString(ResourceKeys.OverflowText));

            foreach (var item in _items)
            {
                var value = FormatCurrency(item.CurrentValue);
                var length = value.Length;
                var padLeft = length <= 10 ? 7 : 7 - (length - 7) / 2 < 0 ? 0 : 7 - (length - 7) / 2;
                var midVal = $"{value} {item.Residual.ToString().PadLeft(padLeft)}";
                AddLine(
                    item.LevelName,
                    midVal,
                    item.Overflow.ToString());
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