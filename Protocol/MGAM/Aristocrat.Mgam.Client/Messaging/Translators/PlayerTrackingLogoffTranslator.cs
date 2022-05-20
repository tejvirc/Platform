namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.PlayerTrackingLogoff"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.PlayerTrackingLogoff"/>.
    /// </summary>
    public class PlayerTrackingLogoffTranslator : MessageTranslator<Messaging.PlayerTrackingLogoff>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.PlayerTrackingLogoff message)
        {
            return new PlayerTrackingLogoff
            {
                InstanceID = new PlayerTrackingLogoffInstanceID { Value = message.InstanceId }
            };
        }
    }
}
