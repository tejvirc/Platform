namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Model;

    /// <summary>
    ///     Represents the option config complex parameter.
    /// </summary>
    [DisplayName("complex")]
    public class OptionConfigComplexParameter : OptionConfigParameter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionConfigComplexParameter" /> class.
        /// </summary>
        public OptionConfigComplexParameter()
        {
            ParameterType = OptionConfigParameterType.Complex;
        }

        /// <summary>
        ///     Gets or sets an option config child parameters
        /// </summary>
        public IEnumerable<OptionConfigParameter> Items { get; set; }
    }
}