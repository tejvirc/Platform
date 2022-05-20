namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.EmployeeLogin"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.EmployeeLogin"/>.
    /// </summary>
    public class EmployeeLoginTranslator : MessageTranslator<Messaging.EmployeeLogin>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.EmployeeLogin message)
        {
            return new EmployeeLogin
            {
                InstanceID = new EmployeeLoginInstanceID { Value = message.InstanceId },
                CardString = new EmployeeLoginCardString { Value = message.CardString },
                PIN = new EmployeeLoginPIN { Value = message.Pin }
            };
        }
    }
}
