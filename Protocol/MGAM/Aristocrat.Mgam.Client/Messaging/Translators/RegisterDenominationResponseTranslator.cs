namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterDenominationResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterDenominationResponse"/>.
    /// </summary>
    public class RegisterDenominationResponseTranslator : MessageTranslator<Protocol.RegisterDenominationResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RegisterDenominationResponse message)
        {
            return new RegisterDenominationResponse { ResponseCode = (ServerResponseCode)message.ResponseCode.Value };
        }
    }
}
