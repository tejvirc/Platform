namespace Aristocrat.Mgam.Client.Messaging
{
    using Client.Command;

    /// <summary>
    ///     This message is used by the VLT to register commands with the VLT service.
    /// </summary>
    public class RegisterCommand : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets and sets the command ID.
        /// </summary>
        public int CommandId { get; set; }

        /// <summary>
        ///     Gets or sets the command description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the command parameter name.
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        ///     Gets or sets the command minimum value.
        /// </summary>
        public string Minimum { get; set; }

        /// <summary>
        ///     Gets or sets the command maximum value.
        /// </summary>
        public string Maximum { get; set; }

        /// <summary>
        ///     Gets the command allowed values.
        /// </summary>
        public string[] AllowedValues { get; set; }

        /// <summary>
        ///     Gets or sets the command default value.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        ///     Gets or sets the command value type.
        /// </summary>
        public CommandValueType Type { get; set; }

        /// <summary>
        ///     Gets or sets the command control type.
        /// </summary>
        public CommandControlType ControlType { get; set; }

        /// <summary>
        ///     Gets or sets the command access type.
        /// </summary>
        public CommandAccessType AccessType { get; set; }
    }
}
