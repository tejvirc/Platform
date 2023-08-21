namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.BeginSessionWithCash"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.BeginSessionWithCash"/> instance.
    /// </summary>
    public class BeginSessionWithCashTranslator : MessageTranslator<Messaging.BeginSessionWithCash>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.BeginSessionWithCash message)
        {
            return new BeginSessionWithCash
            {
                InstanceID = new BeginSessionWithCashInstanceID
                {
                    Value = message.InstanceId
                },
                Amount = new BeginSessionWithCashAmount
                {
                    Value = message.Amount
                }
            };
        }
    }
}
