namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RequestGuid"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RequestGUID"/> instance.
    /// </summary>
    public class RequestGuidTranslator : MessageTranslator<RequestGuid>
    {
        /// <inheritdoc />
        public override object Translate(RequestGuid message)
        {
            return new RequestGUID
            {
                ResponseAddress = new RequestGUIDResponseAddress
                {
                    type = "string",
                    Value = message.ResponseAddress.ToString()
                }
            };
        }
    }
}
