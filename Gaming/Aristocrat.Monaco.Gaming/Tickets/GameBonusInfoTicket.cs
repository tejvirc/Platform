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
        private readonly IEnumerable<BonusInfoMeter> _items;
        private readonly string _bonusInfoCategoryLabel;
        private readonly string _bonusInfoCategoryTotalLabel;

        public GameBonusInfoTicket((string CategoryKey, string CategoryTotalKey) categoryLabelKeys, IEnumerable<BonusInfoMeter> items)
            : base(Localizer.For(CultureFor.OperatorTicket))
        {
            _items = items;
            _bonusInfoCategoryLabel = TicketLocalizer.GetString(categoryLabelKeys.CategoryKey);
            _bonusInfoCategoryTotalLabel = TicketLocalizer.GetString(categoryLabelKeys.CategoryTotalKey);
            Title = TicketLocalizer.GetString(ResourceKeys.GameBonusInfoTicketTitleText).ToUpper(TicketLocalizer.CurrentCulture);
        }

        public override void AddTicketContent()
        {
            if (_items is null || !_items.Any())
            {
                return;
            }

            AddDashesLine();
            AddLine(null, _bonusInfoCategoryLabel, null);

            AddLabeledLine(
                ResourceKeys.NameLabel,
                ResourceKeys.Value);

            var totalValue = 0L;
            foreach (var item in _items)
            {
                var label = item.MeterName;
                var value = item.MeterValue;
                var valueString = value.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture);
                var isLabelStringLonger = label.Length > valueString.Length;
                AddLabeledLine(item.MeterName, valueString, false, isLabelStringLonger);

                totalValue += value;
            }

            var totalValueString = totalValue.FormattedCurrencyString(culture: TicketLocalizer.CurrentCulture);
            var isTotalLabelStringLonger = _bonusInfoCategoryTotalLabel.Length > totalValueString.Length;
            AddLabeledLine(_bonusInfoCategoryTotalLabel, totalValueString, false, isTotalLabelStringLonger);

            AddDashesLine();
            AddTicketFooter();
        }
    }
}