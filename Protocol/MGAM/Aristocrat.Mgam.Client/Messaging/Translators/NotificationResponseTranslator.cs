namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.NotificationResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.NotificationResponse"/>.
    /// </summary>
    public class NotificationResponseTranslator : MessageTranslator<Protocol.NotificationResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.NotificationResponse message)
        {
            return new NotificationResponse { ResponseCode = (ServerResponseCode)message.ResponseCode.Value };
        }
    }
}
