namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterCommandResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterCommandResponse"/> instance.
    /// </summary>
    public class RegisterCommandResponseTranslator : MessageTranslator<Protocol.RegisterCommandResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RegisterCommandResponse message)
        {
            return new RegisterCommandResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value
            };
        }
    }
}
