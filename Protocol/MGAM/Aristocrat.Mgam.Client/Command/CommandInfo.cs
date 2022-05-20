namespace Aristocrat.Mgam.Client.Command
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a command.
    /// </summary>
    public struct CommandInfo
    {
        /// <summary>
        ///     Gets or sets the numeric identifier for this command.
        /// </summary>
        public int CommandId { get; set; }

        /// <summary>
        ///     Gets of sets the human-friendly description of this command.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the optional minimum value for numeric parameter commands.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public object Minimum { get; set; }

        /// <summary>
        ///     Gets or sets the optional maximum value for numeric parameter commands.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public object Maximum { get; set; }

        /// <summary>
        ///     Gets are sets the type of the parameter for this command.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public CommandValueType Type { get; set; }

        /// <summary>
        ///     Gets or sets the GUI control type for the command parameter.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public CommandControlType ControlType { get; set; }

        /// <summary>
        ///     Gets are sets the allowed access types.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public CommandAccessType AccessType { get; set; }

        /// <summary>
        ///     Gets or sets the human-friendly description of the command parameter.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string ParameterName { get; set; }

        /// <summary>
        ///     Gets are sets the optional list of allowed values, pipe '|' delimited.
        /// </summary>
        public IList<object> AllowedValues { get; set; }

        /// <summary>
        ///     Gets or sets the initial value for the parameter of this command.
        /// </summary>
        public object DefaultValue { get; set; }
    }
}
