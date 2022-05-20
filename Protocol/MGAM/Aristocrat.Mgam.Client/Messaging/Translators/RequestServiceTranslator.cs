namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RequestService"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RequestService"/>.
    /// </summary>
    public class RequestServiceTranslator : MessageTranslator<Messaging.RequestService>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.RequestService message)
        {
            return new RequestService
            {
                ServiceName = new RequestServiceServiceName
                {
                    Value = message.ServiceName
                },
                ResponseAddress = new RequestServiceResponseAddress
                {
                    type = "string",
                    Value = message.ResponseAddress.ToString()
                }
            };
        }
    }
}
