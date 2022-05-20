namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RequestPlayResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RequestPlayResponse"/>.
    /// </summary>
    public class RequestPlayResponseTranslator : MessageTranslator<Protocol.RequestPlayResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RequestPlayResponse message)
        {
            bool.TryParse(message.IsProgressiveWin.Value, out var isProgressiveWin);

            return new RequestPlayResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                PrizeIndex = message.PrizeIndex.Value,
                PrizeValue = message.PrizeValue.Value,
                ExtendedInfo = message.ExtendedInfo.Value,
                SessionCashBalance = message.SessionCashBalance.Value,
                SessionCouponBalance = message.SessionCouponBalance.Value,
                IsProgressiveWin = isProgressiveWin,
                ServerTransactionId = message.ServerTransactionID.Value,
                ProgressivePrizeValue = message.ProgressivePrizeValue.Value
            };
        }
    }
}