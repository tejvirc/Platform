namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.UnregisterInstanceResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.UnregisterInstanceResponse"/> instance.
    /// </summary>
    public class UnregisterInstanceResponseTranslator : MessageTranslator<Protocol.UnregisterInstanceResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.UnregisterInstanceResponse message)
        {
            return new UnregisterInstanceResponse { ResponseCode = (ServerResponseCode)message.ResponseCode.Value};
        }
    }
}
