namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.CreditCash"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.CreditCash"/>.
    /// </summary>
    public class CreditCashTranslator : MessageTranslator<Messaging.CreditCash>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.CreditCash message)
        {
            return new CreditCash
            {
                InstanceID = new CreditCashInstanceID
                {
                    Value = message.InstanceId
                },
                SessionID = new CreditCashSessionID
                {
                    Value = message.SessionId
                },
                Amount = new CreditCashAmount
                {
                    Value = message.Amount
                },
                LocalTransactionID = new CreditCashLocalTransactionID
                {
                    Value = message.LocalTransactionId
                }
            };
        }
    }
}
