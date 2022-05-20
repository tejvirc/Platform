namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RequestPlayVoucherResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RequestPlayVoucherResponse"/>.
    /// </summary>
    public class RequestPlayVoucherResponseTranslator : MessageTranslator<Protocol.RequestPlayVoucherResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RequestPlayVoucherResponse message)
        {
            bool.TryParse(message.IsProgressiveWin.Value, out var isProgressiveWin);

            return new RequestPlayVoucherResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                PrizeIndex = message.PrizeIndex.Value,
                PrizeValue = message.PrizeValue.Value,
                ExtendedInfo = message.ExtendedInfo.Value,
                SessionCashBalance = message.SessionCashBalance.Value,
                SessionCouponBalance = message.SessionCouponBalance.Value,
                IsProgressiveWin = isProgressiveWin,
                ServerTransactionId = message.ServerTransactionID.Value,
                ProgressivePrizeValue = message.ProgressivePrizeValue.Value,
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