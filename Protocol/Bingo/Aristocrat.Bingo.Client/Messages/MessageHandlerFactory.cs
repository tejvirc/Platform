namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleInjector;

    public class MessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly Container _container;

        public MessageHandlerFactory(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public async Task<TResponse> Handle<TResponse, TMessage>(TMessage message, CancellationToken token)
            where TResponse : IResponse
            where TMessage : IMessage
        {
            return await _container.GetInstance<IMessageHandler<TResponse, TMessage>>().Handle(message, token);
        }
    }
}