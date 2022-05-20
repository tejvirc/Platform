namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.PlayerTrackingLoginResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.PlayerTrackingLoginResponse"/>.
    /// </summary>
    public class PlayerTrackingLoginResponseTranslator : MessageTranslator<Protocol.PlayerTrackingLoginResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.PlayerTrackingLoginResponse message)
        {
            return new PlayerTrackingLoginResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                PlayerName = message.PlayerName.Value,
                PlayerPoints = message.PlayerPoints.Value,
                PromotionalInfo = message.PromotionalInfo.Value
            };
        }
    }
}
