namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.EndSessionResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.EndSessionResponse"/>.
    /// </summary>
    public class EndSessionResponseTranslator : MessageTranslator<Protocol.EndSessionResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.EndSessionResponse message)
        {
            return new EndSessionResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                VoucherBarcode = message.VoucherBarcode.Value,
                CasinoName = message.CasinoName.Value,
                CasinoAddress = message.CasinoAddress.Value,
                VoucherType = message.VoucherType.Value,
                CashAmount = message.CashAmount.Value,
                CouponAmount = message.CouponAmount.Value,
                TotalAmount = message.TotalAmount.Value,
                AmountLongForm = message.AmountLongForm.Value,
                Date = message.Date.Value,
                Time = message.Time.Value,
                Expiration = message.Expiration.Value,
                Id = message.ID.Value
            };
        }
    }
}