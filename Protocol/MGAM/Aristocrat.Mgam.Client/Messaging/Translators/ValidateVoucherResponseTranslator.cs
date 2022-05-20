namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.ValidateVoucherResponse"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Messaging.ValidateVoucherResponse"/> instance.
    /// </summary>
    public class ValidateVoucherResponseTranslator : MessageTranslator<Protocol.ValidateVoucherResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.ValidateVoucherResponse message)
        {
            return new ValidateVoucherResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                VoucherCashValue =  message.VoucherCashValue.Value,
                VoucherCouponValue = message.VoucherCouponValue.Value
            };
        }
    }
}