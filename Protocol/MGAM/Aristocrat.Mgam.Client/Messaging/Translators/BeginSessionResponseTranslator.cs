namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.BeginSessionResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.BeginSessionResponse"/>.
    /// </summary>
    public class BeginSessionResponseTranslator : MessageTranslator<Protocol.BeginSessionResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.BeginSessionResponse message)
        {
            return new BeginSessionResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                SessionId = message.SessionID.Value,
                SessionCashBalance = message.SessionCashBalance.Value,
                SessionCouponBalance = message.SessionCouponBalance.Value,
                OffLineVoucherBarcode = message.OffLineVoucherBarcode.Value
            };
        }
    }
}