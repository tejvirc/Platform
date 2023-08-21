namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.ComponentModel;
    using Model;

    /// <summary>
    ///     Represents the option config decimal parameter.
    /// </summary>
    [DisplayName("decimal")]
    public class OptionConfigDecimalParameter : OptionConfigParameter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigDecimalParameter" /> class.
        /// </summary>
        public OptionConfigDecimalParameter()
        {
            ParameterType = OptionConfigParameterType.Decimal;
        }

        /// <summary>
        ///     Gets or sets a minimum value allowed.
        /// </summary>
        public decimal MinInclude { get; set; }

        /// <summary>
        ///     Gets or sets a maximum value allowed. Null implies no maximum.
        /// </summary>
        public decimal MaxInclude { get; set; }

        /// <summary>
        ///     Gets or sets a maximum number of fractional decimal places.
        /// </summary>
        public int Fractional { get; set; }
    }
}