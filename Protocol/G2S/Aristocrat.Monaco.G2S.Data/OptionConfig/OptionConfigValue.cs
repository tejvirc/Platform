namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.Collections.Generic;
    using Model;

    /// <summary>
    ///     Base model for OptionConfigCurrentValue and OptionConfigDefaultValue.
    /// </summary>
    public class OptionConfigValue
    {
        /// <summary>
        ///     Gets or sets value type.
        /// </summary>
        public OptionConfigValueType ValueType { get; set; }

        /// <summary>
        ///     Gets or sets config value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets the parameter identifier.
        /// </summary>
        public string ParameterId { get; set; }

        /// <summary>
        ///     Gets or sets an option config children values.
        /// </summary>
        public IEnumerable<OptionConfigValue> Items { get; set; }
    }
}