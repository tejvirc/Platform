namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;

    /// <summary>
    ///     Description of option item parameter with values.
    /// </summary>
    public class ParameterDescription
    {
        /// <summary>
        ///     Gets or sets the parameter identifier.
        /// </summary>
        public string ParamId { get; set; }

        /// <summary>
        ///     Gets or sets the name of the parameter.
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        ///     Gets or sets the parameter help.
        /// </summary>
        public string ParamHelp { get; set; }

        /// <summary>
        ///     Gets or sets the parameter instance.
        /// </summary>
        public Func<dynamic> ParamCreator { get; set; }

        /// <summary>
        ///     Gets or sets the parameter instance.
        /// </summary>
        public Func<dynamic> ValueCreator { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public dynamic Value { get; set; }

        /// <summary>
        ///     Gets or sets the default value.
        /// </summary>
        public dynamic DefaultValue { get; set; }
    }
}