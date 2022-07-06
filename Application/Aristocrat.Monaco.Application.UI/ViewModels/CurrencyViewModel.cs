namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;

    using Contracts.Currency;

    public class CurrencyViewModel
    {
        protected readonly Currency Currency;


        public CurrencyViewModel(Currency currency)
        {
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        }

        public string IsoCode => Currency.IsoCode;

        public string Description => Currency.Description;

        /// <summary>
        /// Currency display value
        /// </summary>
        public virtual string DisplayText => Currency.DescriptionWithMinorSymbol;
        
    }
}
