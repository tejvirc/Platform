namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts to <see cref="T:Aristocrat.Mgam.Client.Protocol.CommandResponse"/>.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class CommandResponseTranslator<TMessage> : MessageTranslator<TMessage>
        where TMessage : class, IResponse
    {
        /// <inheritdoc />
        public override object Translate(TMessage message)
        {
            return new CommandResponse
            {
                ResponseCode = new CommandResponseResponseCode
                {
                    Value = (int)message.ResponseCode
                }
            };
        }
    }
}
