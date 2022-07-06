namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;

    using Contracts.Currency;

    public class CurrencyViewModel
    {
#pragma warning disable CS3008 // Identifier is not CLS-compliant
        protected readonly Currency _currency;
#pragma warning restore CS3008 // Identifier is not CLS-compliant

        public CurrencyViewModel(Currency currency)
        {
            _currency = currency ?? throw new ArgumentNullException(nameof(currency));
        }

        public string IsoCode => _currency.IsoCode;

        public string Description => _currency.Description;

        /// <summary>
        /// Currency display value
        /// </summary>
        public virtual string DisplayText => _currency.DescriptionWithMinorSymbol;
        
    }
}
