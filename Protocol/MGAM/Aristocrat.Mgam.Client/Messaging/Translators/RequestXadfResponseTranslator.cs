namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RequestXADFResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RequestXADFResponse"/>.
    /// </summary>
    public class RequestXadfResponseTranslator : MessageTranslator<Protocol.RequestXADFResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RequestXADFResponse message)
        {
            return new RequestXadfResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                DownloadServer = message.DownloadServer.Value,
                Xadf = message.XADF.Value
            };
        }
    }
}
