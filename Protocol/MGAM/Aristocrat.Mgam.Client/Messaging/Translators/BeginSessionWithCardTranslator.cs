namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.BeginSessionWithCard"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.BeginSessionWithCard"/> instance.
    /// </summary>
    public class BeginSessionWithCardTranslator : MessageTranslator<Messaging.BeginSessionWithCard>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.BeginSessionWithCard message)
        {
            return new BeginSessionWithCard
            {
                InstanceID = new BeginSessionWithCardInstanceID
                {
                    Value = message.InstanceId
                },
                CardString = new BeginSessionWithCardCardString
                {
                    Value = message.CardString
                }
            };
        }
    }
}
