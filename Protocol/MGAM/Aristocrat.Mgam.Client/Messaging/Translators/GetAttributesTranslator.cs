namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.GetAttributes"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.GetAttributes"/> instance.
    /// </summary>
    public class GetAttributesTranslator : MessageTranslator<Messaging.GetAttributes>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.GetAttributes message)
        {
            return new GetAttributes
            {
                InstanceID = new GetAttributesInstanceID { Value = message.InstanceId }
            };
        }
    }
}
