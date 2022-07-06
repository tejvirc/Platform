namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    using System.Globalization;

    using Extensions;


    /// <summary>
    /// No currency format
    /// </summary>
    public class NoCurrencyFormat
    {
        /// <summary>
        /// The currency id
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Currency group separator
        /// </summary>
        public string GroupSeparator
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string DecimalSeparator
        {
            get;
            set;
        }


        /// <summary>
        /// The format of No Currency option
        /// </summary>
        public string FormatString
        {
            get
            {
                CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                culture.ApplyNoCurrencyFormat(this);
                return CurrencyExtensions.DefaultDescriptionAmount.FormattedCurrencyString(false, culture);
            }
        }
    }
}
