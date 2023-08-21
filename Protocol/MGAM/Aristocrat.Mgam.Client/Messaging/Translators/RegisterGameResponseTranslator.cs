namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterGameResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterGameResponse"/> instance.
    /// </summary>
    public class RegisterGameResponseTranslator : MessageTranslator<Protocol.RegisterGameResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RegisterGameResponse message)
        {
            return new RegisterGameResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value
            };
        }
    }
}
