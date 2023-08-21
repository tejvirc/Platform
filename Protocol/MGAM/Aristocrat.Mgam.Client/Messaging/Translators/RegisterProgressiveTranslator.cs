namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterProgressive"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterProgressive"/>.
    /// </summary>
    public class RegisterProgressiveTranslator : MessageTranslator<Messaging.RegisterProgressive>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.RegisterProgressive message)
        {
            return new RegisterProgressive
            {
                InstanceID = new RegisterProgressiveInstanceID { Value = message.InstanceId },
                ProgressiveName = new RegisterProgressiveProgressiveName { Value = message.ProgressiveName },
                TicketCost = new RegisterProgressiveTicketCost { Value = message.TicketCost.ToString() },
                SignMessageAttributeName =
                    new RegisterProgressiveSignMessageAttributeName { Value = message.SignMessageAttributeName },
                SignValueAttributeName =
                    new RegisterProgressiveSignValueAttributeName { Value = message.SignValueAttributeName }
            };
        }
    }
}
