namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Monaco.Common;
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterCommand"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterCommand"/> instance.
    /// </summary>
    public class RegisterCommandTranslator : MessageTranslator<Messaging.RegisterCommand>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.RegisterCommand message)
        {
            return new RegisterCommand
            {
                InstanceID = new RegisterCommandInstanceID
                {
                    Value = message.InstanceId
                },
                CommandID = new RegisterCommandCommandID
                {
                    Value = message.CommandId
                },
                Description = new RegisterCommandDescription
                {
                    Value = message.Description
                },
                ParameterName = new RegisterCommandParameterName
                {
                    Value = message.ParameterName
                },
                Minimum = new RegisterCommandMinimum
                {
                    Value = message.Minimum
                },
                Maximum = new RegisterCommandMaximum
                {
                    Value = message.Maximum
                },
                AllowedValues = new RegisterCommandAllowedValues
                {
                    Value = message.AllowedValues == null ? string.Empty : string.Join("|", message.AllowedValues)
                },
                DefaultValue = new RegisterCommandDefaultValue
                {
                    Value = message.DefaultValue
                },
                Type = new RegisterCommandType
                {
                    Value = message.Type.GetDescription()
                },
                ControlType = new RegisterCommandControlType
                {
                    Value = message.ControlType.GetDescription()
                },
                AccessType = new RegisterCommandAccessType
                {
                    Value = (int)message.AccessType
                }
            };
        }
    }
}
