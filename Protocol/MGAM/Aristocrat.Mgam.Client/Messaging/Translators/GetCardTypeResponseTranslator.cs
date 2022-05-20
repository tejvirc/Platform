namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.GetCardTypeResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.GetCardTypeResponse"/>.
    /// </summary>
    public class GetCardTypeResponseTranslator : MessageTranslator<Protocol.GetCardTypeResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.GetCardTypeResponse message)
        {
            return new GetCardTypeResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                CardString = message.CardString.Value,
                CardType = message.CardType.Value
            };
        }
    }
}
