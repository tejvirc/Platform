namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.ChecksumResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.ChecksumResponse"/> instance.
    /// </summary>
    public class ChecksumResponseTranslator : MessageTranslator<Protocol.ChecksumResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.ChecksumResponse message)
        {
            return new ChecksumResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value
            };
        }
    }
}