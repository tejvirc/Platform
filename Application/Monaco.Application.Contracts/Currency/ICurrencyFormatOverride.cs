namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    /// <summary>
    ///     The interface of currency override format
    /// </summary>
    public interface ICurrencyFormatOverride
    {
        /// <remarks/>
        int PositivePattern { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        bool PositivePatternSpecified { get; set; }

        /// <remarks/>
        int NegativePattern { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        bool NegativePatternSpecified { get; set; }

        /// <remarks/>
        int DecimalDigits { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        bool DecimalDigitsSpecified { get; set; }

        /// <remarks/>
        string DecimalSeparator { get; set; }

        /// <remarks/>
        string GroupSeparator { get; set; }

        /// <remarks/>
        string Symbol { get; set; }

        /// <remarks/>
        string MinorUnitSymbol { get; set; }

        /// <remarks/>
        string MinorUnits { get; set; }

        /// <remarks/>
        string MinorUnitsPlural { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        string[] ExcludePluralizeMajorUnits { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        string[] ExcludePluralizeMinorUnits { get; set; }
    }
}
