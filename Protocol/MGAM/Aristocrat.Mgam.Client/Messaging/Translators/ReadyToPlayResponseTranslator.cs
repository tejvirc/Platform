namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.ReadyToPlayResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.ReadyToPlayResponse"/>.
    /// </summary>
    public class ReadyToPlayResponseTranslator : MessageTranslator<Protocol.ReadyToPlayResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.ReadyToPlayResponse message)
        {
            return new ReadyToPlayResponse { ResponseCode = (ServerResponseCode)message.ResponseCode.Value };
        }
    }
}
