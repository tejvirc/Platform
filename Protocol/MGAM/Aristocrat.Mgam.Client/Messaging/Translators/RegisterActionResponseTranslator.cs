namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterActionResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterActionResponse"/>.
    /// </summary>
    public class RegisterActionResponseTranslator : MessageTranslator<Protocol.RegisterActionResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RegisterActionResponse message)
        {
            return new RegisterActionResponse { ResponseCode = (ServerResponseCode)message.ResponseCode.Value };
        }
    }
}
