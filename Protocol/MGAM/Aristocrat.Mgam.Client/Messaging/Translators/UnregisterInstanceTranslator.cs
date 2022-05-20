namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.UnregisterInstance"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.UnregisterInstance"/>.
    /// </summary>
    public class UnregisterInstanceTranslator : MessageTranslator<Messaging.UnregisterInstance>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.UnregisterInstance message)
        {
            return new UnregisterInstance
            {
                InstanceID = new UnregisterInstanceInstanceID
                {
                    Value = message.InstanceId
                }
            };
        }
    }
}
