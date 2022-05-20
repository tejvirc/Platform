namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using System;
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RequestGUIDResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RequestGuidResponse"/>.
    /// </summary>
    public class RequestGuidResponseTranslator : MessageTranslator<RequestGUIDResponse>
    {
        /// <inheritdoc />
        public override object Translate(RequestGUIDResponse message)
        {
            return new RequestGuidResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                Guid = Guid.Parse(message.GUID.Value)
            };
        }
    }
}
