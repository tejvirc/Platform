namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.BeginSession"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.BeginSession"/>.
    /// </summary>
    public class BeginSessionTranslator : MessageTranslator<Messaging.BeginSession>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.BeginSession message)
        {
            return new BeginSession
            {
                InstanceID = new BeginSessionInstanceID
                {
                    Value = message.InstanceId
                }
            };
        }
    }
}
