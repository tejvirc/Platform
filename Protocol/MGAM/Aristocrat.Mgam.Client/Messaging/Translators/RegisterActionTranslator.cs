namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterAction"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterAction"/>.
    /// </summary>
    public class RegisterActionTranslator : MessageTranslator<Messaging.RegisterAction>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.RegisterAction message)
        {
            return new RegisterAction
            {
                InstanceID = new RegisterActionInstanceID { Value = message.InstanceId },
                ActionGUID = new RegisterActionActionGUID { Value = message.ActionGuid.ToString() },
                ActionName = new RegisterActionActionName { Value = message.Name },
                ActionDescription = new RegisterActionActionDescription { Value = message.Description }
            };
        }
    }
}
