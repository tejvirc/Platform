namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using System;

    /// <summary>
    ///     
    /// </summary>
    public class MessageTranslatorFactory : IMessageTranslatorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageTranslatorFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public MessageTranslatorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
            
        /// <inheritdoc />
        public IMessageTranslator<TMessage> Create<TMessage>()
            where TMessage : class
        {
            return _serviceProvider.GetRequiredService<IMessageTranslator<TMessage>>();
        }

        /// <inheritdoc />
        public IMessageTranslator Create(Type messageType)
        {
            var translatorType = typeof(IMessageTranslator<>).MakeGenericType(messageType);

            return (IMessageTranslator)_serviceProvider.GetRequiredService(translatorType);
        }
    }
}
