namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.ComponentModel;
    using Model;

    /// <summary>
    ///     Represents the option config boolean parameter.
    /// </summary>
    [DisplayName("bool")]
    public class OptionConfigBooleanParameter : OptionConfigParameter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigBooleanParameter" /> class.
        /// </summary>
        public OptionConfigBooleanParameter()
        {
            ParameterType = OptionConfigParameterType.Boolean;
        }
    }
}