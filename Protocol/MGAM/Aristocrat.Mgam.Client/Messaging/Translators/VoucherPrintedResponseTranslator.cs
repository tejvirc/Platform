namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.VoucherPrintedResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.VoucherPrintedResponse"/> instance.
    /// </summary>
    public class VoucherPrintedResponseTranslator : MessageTranslator<Protocol.VoucherPrintedResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.VoucherPrintedResponse message)
        {
            return new VoucherPrintedResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value
            };
        }
    }
}