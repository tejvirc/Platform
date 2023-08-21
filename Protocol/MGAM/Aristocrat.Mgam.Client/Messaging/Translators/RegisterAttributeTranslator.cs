namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Monaco.Common;
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterAttribute"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterAttribute"/> instance.
    /// </summary>
    public class RegisterAttributeTranslator : MessageTranslator<Messaging.RegisterAttribute>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.RegisterAttribute message)
        {
            return new RegisterAttribute
            {
                InstanceID = new RegisterAttributeInstanceID
                {
                    Value = message.InstanceId
                },
                Scope = new RegisterAttributeScope
                {
                    Value = message.Scope.GetDescription()
                },
                ItemName = new RegisterAttributeItemName
                {
                    Value = message.ItemName
                },
                ItemValue = new RegisterAttributeItemValue
                {
                    Value = message.ItemValue
                },
                Minimum = new RegisterAttributeMinimum
                {
                    Value = message.Minimum
                },
                Maximum = new RegisterAttributeMaximum
                {
                    Value = message.Maximum
                },
                AllowedValues = new RegisterAttributeAllowedValues
                {
                    Value = message.AllowedValues == null ? string.Empty : string.Join("|", message.AllowedValues)
                },
                Type = new RegisterAttributeType
                {
                    Value = message.Type.GetDescription()
                },
                ControlType = new RegisterAttributeControlType
                {
                    Value = message.ControlType.GetDescription()
                },
                AccessType = new RegisterAttributeAccessType
                {
                    Value = (int)message.AccessType
                }
            };
        }
    }
}
