namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.PlayerTrackingLogin"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.PlayerTrackingLogin"/>.
    /// </summary>
    public class PlayerTrackingLoginTranslator : MessageTranslator<Messaging.PlayerTrackingLogin>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.PlayerTrackingLogin message)
        {
            return new PlayerTrackingLogin
            {
                InstanceID = new PlayerTrackingLoginInstanceID { Value = message.InstanceId },
                PlayerTrackingString = new PlayerTrackingLoginPlayerTrackingString { Value = message.PlayerTrackingString }
            };
        }
    }
}
