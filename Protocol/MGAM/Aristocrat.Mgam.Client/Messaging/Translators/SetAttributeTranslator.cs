namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.SetAttribute"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.SetAttribute"/>.
    /// </summary>
    public class SetAttributeTranslator : MessageTranslator<Messaging.SetAttribute>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.SetAttribute message)
        {
            return new SetAttribute
            {
                InstanceID = new SetAttributeInstanceID { Value = message.InstanceId },
                Scope = new SetAttributeScope { Value = message.Scope.ToString().ToLower() },
                AttributeName = new SetAttributeAttributeName { Value = message.Name },
                AttributeValue = new SetAttributeAttributeValue { Value = message.Value }
            };
        }
    }
}
