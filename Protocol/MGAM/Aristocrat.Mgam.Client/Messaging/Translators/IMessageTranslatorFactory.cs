namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using System;

    /// <summary>
    ///     
    /// </summary>
    public interface IMessageTranslatorFactory
    {
        /// <summary>
        ///     
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <returns></returns>
        IMessageTranslator<TMessage> Create<TMessage>()
            where TMessage : class;

        /// <summary>
        ///     
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        IMessageTranslator Create(Type messageType);
    }
}
