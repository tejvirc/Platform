namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.KeepAlive"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.KeepAlive"/>.
    /// </summary>
    public class KeepAliveTranslator : MessageTranslator<Messaging.KeepAlive>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.KeepAlive message)
        {
            return new KeepAlive { InstanceID = new KeepAliveInstanceID { Value = message.InstanceId } };
        }
    }
}
