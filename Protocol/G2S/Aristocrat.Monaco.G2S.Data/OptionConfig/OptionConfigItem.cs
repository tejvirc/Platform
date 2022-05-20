namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using Common.Storage;
    using Model;
    using Newtonsoft.Json;

    /// <summary>
    ///     Represents record from OptionConfigItem data table.
    /// </summary>
    public class OptionConfigItem : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the option identifier.
        /// </summary>
        /// <value>
        ///     The option identifier.
        /// </value>
        public string OptionId { get; set; }

        /// <summary>
        ///     Gets or sets the security level.
        /// </summary>
        /// <value>
        ///     The security level.
        /// </value>
        public SecurityLevel SecurityLevel { get; set; }

        /// <summary>
        ///     Gets or sets the minimum selections.
        /// </summary>
        /// <value>
        ///     The minimum selections.
        /// </value>
        public int MinSelections { get; set; }

        /// <summary>
        ///     Gets or sets the maximum selections.
        /// </summary>
        /// <value>
        ///     The maximum selections.
        /// </value>
        public int MaxSelections { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="OptionConfigItem" /> is duplicates.
        /// </summary>
        /// <value>
        ///     <c>true</c> if duplicates; otherwise, <c>false</c>.
        /// </value>
        public bool Duplicates { get; set; }

        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        public string Parameters { get; set; }

        /// <summary>
        ///     Gets or sets the current values.
        /// </summary>
        /// <value>
        ///     The current values.
        /// </value>
        public string CurrentValues { get; set; }

        /// <summary>
        ///     Gets or sets the default values.
        /// </summary>
        /// <value>
        ///     The default values.
        /// </value>
        public string DefaultValues { get; set; }

        /// <summary>
        ///     Gets or sets the option configuration group identifier.
        /// </summary>
        /// <value>
        ///     The option configuration group identifier.
        /// </value>
        public long OptionConfigGroupId { get; set; }

        /// <summary>
        ///     Gets or sets the type of the parameter.
        /// </summary>
        /// <value>
        ///     The type of the parameter.
        /// </value>
        public OptionConfigParameterType ParameterType { get; set; }

        /// <summary>
        ///     Gets or sets the option configuration group.
        /// </summary>
        /// <value>
        ///     The option configuration group.
        /// </value>
        public virtual OptionConfigGroup OptionConfigGroup { get; set; }

        /// <summary>
        ///     Gets the option configuration parameter.
        /// </summary>
        /// <returns>Option config parameter</returns>
        public OptionConfigParameter GetOptionConfigParameter()
        {
            if (ParameterType == OptionConfigParameterType.Integer)
            {
                return JsonConvert.DeserializeObject<OptionConfigIntegerParameter>(Parameters);
            }

            if (ParameterType == OptionConfigParameterType.Decimal)
            {
                return JsonConvert.DeserializeObject<OptionConfigDecimalParameter>(Parameters);
            }

            if (ParameterType == OptionConfigParameterType.String)
            {
                return JsonConvert.DeserializeObject<OptionConfigStringParameter>(Parameters);
            }

            if (ParameterType == OptionConfigParameterType.Boolean)
            {
                return JsonConvert.DeserializeObject<OptionConfigBooleanParameter>(Parameters);
            }

            if (ParameterType == OptionConfigParameterType.Complex)
            {
                return JsonConvert.DeserializeObject<OptionConfigComplexParameter>(Parameters);
            }

            return null;
        }

        /// <summary>
        ///     Gets the option current configuration value.
        /// </summary>
        /// <returns>Current Option config value.</returns>
        public OptionConfigValue GetOptionCurrentConfigValue()
        {
            return JsonConvert.DeserializeObject<OptionConfigValue>(CurrentValues);
        }

        /// <summary>
        ///     Gets the option default configuration value.
        /// </summary>
        /// <returns>Default Option config value.</returns>
        public OptionConfigValue GetOptionDefaultConfigValue()
        {
            return JsonConvert.DeserializeObject<OptionConfigValue>(DefaultValues);
        }
    }
}