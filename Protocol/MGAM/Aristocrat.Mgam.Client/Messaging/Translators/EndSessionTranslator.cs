namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.EndSession"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.EndSession"/>.
    /// </summary>
    public class EndSessionTranslator : MessageTranslator<Messaging.EndSession>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.EndSession message)
        {
            return new EndSession
            {
                InstanceID = new EndSessionInstanceID
                {
                    Value = message.InstanceId
                },
                SessionID = new EndSessionSessionID()
                {
                    Value = message.SessionId
                },
                LocalTransactionID = new EndSessionLocalTransactionID()
                {
                    Value = message.LocalTransactionId
                }
            };
        }
    }
}
