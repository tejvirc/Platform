namespace Aristocrat.Monaco.Application.Localization
{
    using Contracts.Currency;

    public partial class CurrencyDefaultsCurrencyInfoFormat : ICurrencyFormatOverride
    {
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public string[] ExcludePluralizeMajorUnits { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public string[] ExcludePluralizeMinorUnits { get; set; }
    }
}
