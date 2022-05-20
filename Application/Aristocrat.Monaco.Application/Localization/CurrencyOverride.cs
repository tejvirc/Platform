namespace Aristocrat.Monaco.Application.Localization
{

    public partial class CurrencyDefaultsCurrencyInfoFormat : ICurrencyOverride
    {
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public string[] ExcludePluralizeMajorUnits { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public string[] ExcludePluralizeMinorUnits { get; set; }
    }
}
