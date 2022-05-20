namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.SetAttributeResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.SetAttributeResponse"/>.
    /// </summary>
    public class SetAttributeResponseTranslator : MessageTranslator<Protocol.SetAttributeResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.SetAttributeResponse message)
        {
            return new SetAttributeResponse { ResponseCode = (ServerResponseCode)message.ResponseCode.Value };
        }
    }
}
