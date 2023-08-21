namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.EscrowCash"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.EscrowCash"/> instance.
    /// </summary>
    public class EscrowCashTranslator : MessageTranslator<Messaging.EscrowCash>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.EscrowCash message)
        {
            return new EscrowCash
            {
                InstanceID = new EscrowCashInstanceID
                {
                    Value = message.InstanceId
                },
                Amount = new EscrowCashAmount
                {
                    Value = message.Amount
                }
            };
        }
    }
}
