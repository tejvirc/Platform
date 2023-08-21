namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Base class for optionConfig parameters.
    /// </summary>
    public class OptionConfigParameter : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the parameter identifier.
        /// </summary>
        /// <value>
        ///     The parameter identifier.
        /// </value>
        public string ParameterId { get; set; }

        /// <summary>
        ///     Gets or sets the parameter name.
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        ///     Gets or sets the parameter help.
        /// </summary>
        public string ParameterHelp { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the parameter is a component of
        ///     the unique key for an option selection.
        /// </summary>
        public bool ParameterKey { get; set; }

        /// <summary>
        ///     Gets or sets allowed values for the option.
        /// </summary>
        public string AllowedValues { get; set; }

        /// <summary>
        ///     Gets or sets a parameter type.
        /// </summary>
        public OptionConfigParameterType ParameterType { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the parameter can be changed locally
        ///     at the EGM.
        /// </summary>
        public bool CanModLocal { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the parameter can be changed
        ///     remotely via the optionConfig device.
        /// </summary>
        public bool CanModRemote { get; set; }
    }
}