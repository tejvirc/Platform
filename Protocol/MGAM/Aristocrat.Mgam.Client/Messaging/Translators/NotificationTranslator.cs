namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.Notification"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.Notification"/> instance.
    /// </summary>
    public class NotificationTranslator : MessageTranslator<Messaging.Notification>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.Notification message)
        {
            return new Notification
            {
                InstanceID = new NotificationInstanceID
                {
                    Value = message.InstanceId
                },
                NotificationID = new NotificationNotificationID
                {
                    Value = message.NotificationId
                },
                Parameter = new NotificationParameter
                {
                    Value = message.Parameter
                }
            };
        }
    }
}
