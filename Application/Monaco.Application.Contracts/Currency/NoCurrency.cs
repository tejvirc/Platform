namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    using System.Globalization;
    using Extensions;

    /// <summary>
    /// The generic currency without having major and minor symbols
    /// </summary>
    public class NoCurrency : Currency
    {
        /// <summary>
        /// The No Currency Code starts with NC
        /// </summary>
        public const string NoCurrencyCode = "NOC";

        /// <summary>
        /// The currency name for No Currency
        /// </summary>
        public const string NoCurrencyName = "No Currency";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The currency id</param>
        /// <param name="culture">The culture</param>
        public NoCurrency(int id, CultureInfo culture) :
            base(NoCurrencyCode, null, culture, null)
        {
            Id = id;
            IsoCode = NoCurrencyCode;

            Description = culture.GetFormattedDescription(NoCurrencyCode, null);

            Culture = culture;
        }

        /// <summary>
        /// The id of no currency option
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The currency symbol
        /// </summary>
        public override string CurrencySymbol => string.Empty;

        /// <summary>
        /// Minor unit symbol
        /// </summary>
        public override string MinorUnitSymbol => string.Empty;

        /// <summary>
        /// The currency's English name
        /// </summary>
        public override string CurrencyName => NoCurrencyName;
    }
}
