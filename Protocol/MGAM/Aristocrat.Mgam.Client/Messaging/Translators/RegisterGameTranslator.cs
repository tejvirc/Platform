namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterGame"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterGame"/> instance.
    /// </summary>
    public class RegisterGameTranslator : MessageTranslator<Messaging.RegisterGame>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.RegisterGame message)
        {
            return new RegisterGame
            {
                InstanceID = new RegisterGameInstanceID
                {
                    Value = message.InstanceId
                },
                GameUPCNumber = new RegisterGameGameUPCNumber
                {
                    Value = message.GameUpcNumber
                },
                NumberOfCredits = new RegisterGameNumberOfCredits
                {
                    Value = message.NumberOfCredits
                },
                GameDescription = new RegisterGameGameDescription
                {
                    Value = message.GameDescription
                },
                PayTableDescription = new RegisterGamePayTableDescription
                {
                    Value = message.PayTableDescription
                },
                PayTableIndex = new RegisterGamePayTableIndex
                {
                    Value = message.PayTableIndex
                }
            };
        }
    }
}
