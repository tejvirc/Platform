namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.Tickets;
    using Contracts.Bonus;
    using Localization.Properties;

    /// <summary>
    /// </summary>
    public class GameBonusInfoTicket : TextTicket
    {
        private readonly string _bonusInfo;
        private readonly IEnumerable<BonusInfoMeter> _items;
        private readonly string _totalMeterLabelKey;

        public GameBonusInfoTicket(
            string bonusInfo,
            IEnumerable<BonusInfoMeter> items,
            string totalMeterLabelKey)
            : base(Localizer.For(CultureFor.PlayerTicket))
        {
            _items = items;
            _bonusInfo = bonusInfo;
            _totalMeterLabelKey = totalMeterLabelKey;

            Title = TicketLocalizer.GetString(ResourceKeys.GameBonusInfoTicketTitleText).ToUpper(TicketLocalizer.CurrentCulture);
        }

        public override void AddTicketContent()
        {
            if (_items is null || !_items.Any())
            {
                return;
            }

            AddDashesLine();
            AddLine(null, _bonusInfo, null);

            AddLine(
                TicketLocalizer.GetString(ResourceKeys.NameLabel),
                string.Empty,
                TicketLocalizer.GetString(ResourceKeys.Value));

            var totalValue = 0L;
            foreach (var item in _items)
            {
                var value = item.MeterValue;
                var valueString = value.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture);
                AddLine(
                    item.MeterName,
                    null,
                    valueString);
                totalValue += value;
            }

            var totalLabel = TicketLocalizer.GetString(_totalMeterLabelKey);
            AddLine(
                totalLabel,
                null,
                totalValue.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture));


            AddDashesLine();
            AddTicketFooter();
        }
    }
}