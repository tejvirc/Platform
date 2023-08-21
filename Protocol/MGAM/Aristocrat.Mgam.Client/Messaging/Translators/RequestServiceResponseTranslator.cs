namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RequestServiceResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RequestServiceResponse"/>.
    /// </summary>
    public class RequestServiceResponseTranslator : MessageTranslator<Protocol.RequestServiceResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RequestServiceResponse message)
        {
            return new RequestServiceResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                ConnectionString = message.ConnectionString.Value
            };
        }
    }
}
