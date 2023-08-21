namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterDenomination"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterDenomination"/>.
    /// </summary>
    public class RegisterDenominationTranslator : MessageTranslator<Messaging.RegisterDenomination>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.RegisterDenomination message)
        {
            return new RegisterDenomination
            {
                InstanceID = new RegisterDenominationInstanceID { Value = message.InstanceId },
                GameUPCNumber = new RegisterDenominationGameUPCNumber { Value = message.GameUpcNumber },
                Denomination = new RegisterDenominationDenomination { Value = message.Denomination }
            };
        }
    }
}
