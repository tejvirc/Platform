namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterProgressiveResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterProgressiveResponse"/>.
    /// </summary>
    public class RegisterProgressiveResponseTranslator : MessageTranslator<Protocol.RegisterProgressiveResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RegisterProgressiveResponse message)
        {
            return new RegisterProgressiveResponse { ResponseCode = (ServerResponseCode)message.ResponseCode.Value };
        }
    }
}
