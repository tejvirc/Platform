namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.PlayerTrackingLogoffResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.PlayerTrackingLogoffResponse"/>.
    /// </summary>
    public class PlayerTrackingLogoffResponseTranslator : MessageTranslator<Protocol.PlayerTrackingLogoffResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.PlayerTrackingLogoffResponse message)
        {
            return new PlayerTrackingLogoffResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value
            };
        }
    }
}
