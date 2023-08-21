namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.CreditResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.CreditResponse"/> instance.
    /// </summary>
    public class CreditResponseTranslator : MessageTranslator<Protocol.CreditResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.CreditResponse message)
        {
            return new CreditResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                SessionCashBalance = message.SessionCashBalance.Value,
                SessionCouponBalance = message.SessionCouponBalance.Value,
                ServerTransactionId = message.ServerTransactionID.Value
            };
        }
    }
}