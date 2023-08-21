namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.EscrowCashResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.EscrowCashResponse"/> instance.
    /// </summary>
    public class EscrowCashResponseTranslator : MessageTranslator<Protocol.EscrowCashResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.EscrowCashResponse message)
        {
            return new EscrowCashResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value
            };
        }
    }
}