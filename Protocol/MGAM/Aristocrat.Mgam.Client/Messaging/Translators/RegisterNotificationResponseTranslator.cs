namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterNotificationResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterNotificationResponse"/> instance.
    /// </summary>
    public class RegisterNotificationResponseTranslator : MessageTranslator<Protocol.RegisterNotificationResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RegisterNotificationResponse message)
        {
            return new RegisterNotificationResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value
            };
        }
    }
}
