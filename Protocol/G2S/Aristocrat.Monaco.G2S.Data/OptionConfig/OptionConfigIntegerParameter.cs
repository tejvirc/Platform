namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.ComponentModel;
    using Model;

    /// <summary>
    ///     Represents the option config integer parameter.
    /// </summary>
    [DisplayName("integer")]
    public class OptionConfigIntegerParameter : OptionConfigParameter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigIntegerParameter" /> class.
        /// </summary>
        public OptionConfigIntegerParameter()
        {
            ParameterType = OptionConfigParameterType.Integer;
        }

        /// <summary>
        ///     Gets or sets a minimum value allowed.
        /// </summary>
        public long MinInclude { get; set; }

        /// <summary>
        ///     Gets or sets a maximum value allowed. Null implies no maximum.
        /// </summary>
        public long MaxInclude { get; set; }
    }
}