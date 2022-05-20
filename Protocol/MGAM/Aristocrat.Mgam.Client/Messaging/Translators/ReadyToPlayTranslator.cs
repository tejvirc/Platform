namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.ReadyToPlay"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.ReadyToPlay"/>.
    /// </summary>
    public class ReadyToPlayTranslator : MessageTranslator<Messaging.ReadyToPlay>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.ReadyToPlay message)
        {
            return new ReadyToPlay { InstanceID = new ReadyToPlayInstanceID { Value = message.InstanceId } };
        }
    }
}
