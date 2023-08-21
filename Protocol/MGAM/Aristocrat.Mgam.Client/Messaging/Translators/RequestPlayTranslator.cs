namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RequestPlay"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RequestPlay"/>.
    /// </summary>
    public class RequestPlayTranslator : MessageTranslator<Messaging.RequestPlay>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.RequestPlay message)
        {
            return new RequestPlay
            {
                InstanceID = new RequestPlayInstanceID
                {
                    Value = message.InstanceId
                },
                SessionID = new RequestPlaySessionID()
                {
                    Value = message.SessionId
                },
                SessionCashBalance = new RequestPlaySessionCashBalance()
                {
                    Value = message.SessionCashBalance
                },
                SessionCouponBalance = new RequestPlaySessionCouponBalance()
                {
                    Value = message.SessionCouponBalance
                },
                PayTableIndex = new RequestPlayPayTableIndex()
                {
                    Value = message.PayTableIndex
                },
                NumberOfCredits = new RequestPlayNumberOfCredits()
                {
                    Value = message.NumberOfCredits
                },
                Denomination = new RequestPlayDenomination()
                {
                    Value = message.Denomination
                },
                GameUPCNumber = new RequestPlayGameUPCNumber()
                {
                    Value = message.GameUpcNumber
                },
                LocalTransactionID = new RequestPlayLocalTransactionID()
                {
                    Value = message.LocalTransactionId
                }
            };
        }
    }
}
