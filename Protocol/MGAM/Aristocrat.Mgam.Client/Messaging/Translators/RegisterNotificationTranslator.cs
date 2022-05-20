namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterNotification"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterNotification"/> instance.
    /// </summary>
    public class RegisterNotificationTranslator : MessageTranslator<Messaging.RegisterNotification>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.RegisterNotification message)
        {
            return new RegisterNotification
            {
                InstanceID = new RegisterNotificationInstanceID
                {
                    Value = message.InstanceId
                },
                NotificationID = new RegisterNotificationNotificationID
                {
                    Value = message.NotificationId
                },
                Description = new RegisterNotificationDescription
                {
                    Value = message.Description
                },
                PriorityLevel = new RegisterNotificationPriorityLevel
                {
                    Value = message.PriorityLevel
                }
            };
        }
    }
}
