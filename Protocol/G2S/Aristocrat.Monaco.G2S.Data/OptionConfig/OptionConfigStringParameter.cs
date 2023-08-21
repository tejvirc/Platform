namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.ComponentModel;
    using Model;

    /// <summary>
    ///     Represents the option config string parameter.
    /// </summary>
    [DisplayName("string")]
    public class OptionConfigStringParameter : OptionConfigParameter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigStringParameter" /> class.
        /// </summary>
        public OptionConfigStringParameter()
        {
            ParameterType = OptionConfigParameterType.String;
        }

        /// <summary>
        ///     Gets or sets a minimum value allowed.
        /// </summary>
        public int MinLen { get; set; }

        /// <summary>
        ///     Gets or sets a maximum value allowed. Null implies no maximum.
        /// </summary>
        public int MaxLen { get; set; }
    }
}