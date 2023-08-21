namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     
    /// </summary>
    public interface IMessageTranslator
    {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        object Translate(object message);
    }

    /// <summary>
    ///     Converts messages to from XSD generated message objects.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessageTranslator<in TMessage> : IMessageTranslator
        where TMessage : class
    {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        object Translate(TMessage message);
    }
}
