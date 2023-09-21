namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using System.Text.RegularExpressions;
    using Extensions;

    /// <summary>
    /// Currency with its format
    /// </summary>
    public class Currency : ICurrency
    {
        private RegionInfo _region;

        /// <summary>
        /// The default value of currency multiplier
        /// </summary>
        public static readonly long DefaultCurrencyMultiplier = 1M.DollarsToMillicents();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isoCurrencyCode">Currency code</param>
        /// <param name="region">The region</param>
        /// <param name="culture">The culture</param>
        /// <param name="minorUnitSymbol">minor unit symbol</param>
        public Currency(string isoCurrencyCode, RegionInfo region, CultureInfo culture, string minorUnitSymbol = null)
        {
            IsoCode = isoCurrencyCode;
            MinorUnitSymbol = minorUnitSymbol;

            Culture = culture?.Clone() as CultureInfo;
            _region = region;
        }

        /// <summary>
        /// Currency ISO code
        /// </summary>
        public string IsoCode { get; protected set; }

        /// <summary>
        /// Description of currency
        /// </summary>
        public virtual string Description
        {
            get
            {
                return Culture.GetFormattedDescription(IsoCode, _region);
            }
        }

        /// <summary>
        /// The currency symbol
        /// </summary>
        public virtual string CurrencySymbol => Culture.NumberFormat.CurrencySymbol;

        /// <summary>
        /// Minor unit symbol
        /// </summary>
        public virtual string MinorUnitSymbol { get; protected set; }

        /// <summary>
        /// Culture used for the currency format
        /// </summary>
        public virtual CultureInfo Culture { get; set; }

        /// <summary>
        /// The currency's name
        /// </summary>
        public virtual string CurrencyName
        {
            get
            {
                var currencyNameArray = CurrencyEnglishName.Split(' ');
                if (currencyNameArray.Length > 0)
                {
                    return currencyNameArray[currencyNameArray.Length - 1];
                }

                return IsoCode;
            }
            protected set
            {
                throw new NotSupportedException($"{nameof(CurrencyName)} can not be set in Currency.");
            }
        }

        /// <summary>
        /// The currency's english name
        /// </summary>
        public virtual string CurrencyEnglishName
        {
            get
            {
                RegionInfo region = new RegionInfo(Culture.Name);
                return region.EnglishName;
            }
            protected set
            {
                throw new NotSupportedException($"{nameof(CurrencyEnglishName)} can not be set in Currency.");
            }
        }

        /// <summary>
        /// The denom display unit type, default is cent
        /// </summary>
        public virtual DenomDisplayUnitType DenomDisplayUnit => DenomDisplayUnitType.Cent;

        /// <summary>
        ///     Gets Description with minor currency symbol of the current currency.
        /// </summary>
        public virtual string DisplayName
        {
            get
            {
                return string.IsNullOrEmpty(MinorUnitSymbol)
                ? $"{Description}"
                : $"{Description} 10{MinorUnitSymbol}";
            }
            protected set
            {
                throw new NotSupportedException($"{nameof(DisplayName)} can not be set in Currency.");
            }
        }
    }
}