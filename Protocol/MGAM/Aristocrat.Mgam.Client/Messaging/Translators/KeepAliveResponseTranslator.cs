namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a  <see cref="T:Aristocrat.Mgam.Client.Protocol.KeepAliveResponse"/> to <see cref="T:Aristocrat.Mgam.Client.Messaging.KeepAliveResponse"/>.
    /// </summary>
    public class KeepAliveResponseTranslator : MessageTranslator<Protocol.KeepAliveResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.KeepAliveResponse message)
        {
            return new KeepAliveResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value
            };
        }
    }
}
