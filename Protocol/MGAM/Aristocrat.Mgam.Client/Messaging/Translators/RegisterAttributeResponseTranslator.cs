namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterAttributeResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterAttributeResponse"/> instance.
    /// </summary>
    public class RegisterAttributeResponseTranslator : MessageTranslator<Protocol.RegisterAttributeResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RegisterAttributeResponse message)
        {
            return new RegisterAttributeResponse { ResponseCode = (ServerResponseCode)message.ResponseCode.Value };
        }
    }
}
