namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Aristocrat.Monaco.Application.Contracts.Currency;

    public class NoCurrencyViewModel : CurrencyViewModel
    {
        public NoCurrencyViewModel(NoCurrency currency) : base(currency)
        {
        }

        public override string DisplayText => GetNoCurrencyDisplayText();
        
        private string GetNoCurrencyDisplayText()
        {
            string decimalText = _currency.Culture.NumberFormat.CurrencyDecimalDigits > 0 ?
                $"0{_currency.Culture.NumberFormat.CurrencyDecimalSeparator}10" :
                "(no sub-unit)";

            const decimal defaultDescriptionAmount = 1000.00M;
            string amountText = defaultDescriptionAmount.ToString($"C{_currency.Culture.NumberFormat.CurrencyDecimalDigits}", _currency.Culture);

            string displayText = $"{NoCurrency.NoCurrencyName} {amountText} {decimalText}".Trim();
            return displayText;
        }
    }
}
