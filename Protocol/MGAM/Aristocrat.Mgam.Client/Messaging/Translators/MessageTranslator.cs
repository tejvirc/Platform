namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessageTranslator<TMessage> : IMessageTranslator<TMessage>
        where TMessage : class
    {
        /// <inheritdoc />
        public abstract object Translate(TMessage message);

        /// <inheritdoc />
        public object Translate(object message)
        {
            return Translate((TMessage)message);
        }
    }
}
