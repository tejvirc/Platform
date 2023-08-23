namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Windows;
    using Application.Contracts.Extensions;
    using Xceed.Wpf.Toolkit;

    /// <summary>
    ///     CurrencyDecimalUpDown
    ///     DecimalUpDown that displays decimals as formatted currency
    /// </summary>
    public class CurrencyDecimalUpDown : DecimalUpDown
    {
        static CurrencyDecimalUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CurrencyDecimalUpDown),
                new FrameworkPropertyMetadata(typeof(CurrencyDecimalUpDown)));
        }

        /// <inheritdoc />
        protected override string ConvertValueToText()
        {
            return !Value.HasValue ? string.Empty : $"{Value.Value.FormattedCurrencyStringForOperator()}";
        }
    }
}