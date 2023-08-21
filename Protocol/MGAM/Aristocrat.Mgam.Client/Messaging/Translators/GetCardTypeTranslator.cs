namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.GetCardType"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.GetCardType"/>.
    /// </summary>
    public class GetCardTypeTranslator : MessageTranslator<Messaging.GetCardType>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.GetCardType message)
        {
            return new GetCardType
            {
                InstanceID = new GetCardTypeInstanceID { Value = message.InstanceId },
                CardStringTrack1 = new GetCardTypeCardStringTrack1 { Value = message.CardStringTrack1 },
                CardStringTrack2 = new GetCardTypeCardStringTrack2 { Value = message.CardStringTrack2 },
                CardStringTrack3 = new GetCardTypeCardStringTrack3 { Value = message.CardStringTrack3 }
            };
        }
    }
}
