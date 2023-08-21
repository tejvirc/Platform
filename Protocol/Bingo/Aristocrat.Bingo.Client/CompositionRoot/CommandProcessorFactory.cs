namespace Aristocrat.Bingo.Client.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Protobuf.Reflection;
    using Messages.Commands;
    using ServerApiGateway;
    using SimpleInjector;
    using IMessage = Google.Protobuf.IMessage;

    public class CommandProcessorFactory : ICommandProcessorFactory
    {
        private readonly Dictionary<MessageDescriptor, InstanceProducer<ICommandProcessor>>
            _producers = new();

        private readonly Container _container;

        public CommandProcessorFactory(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public Task<IMessage> ProcessCommand(Command command, CancellationToken token)
        {
            var result = _producers.Where(x => command.Command_.Is(x.Key))
                .Select(x => x.Value.GetInstance())
                .FirstOrDefault();
            return result?.ProcessCommand(command, token) ?? Task.FromResult<IMessage>(null);
        }

        public void Register<TImplementation>(MessageDescriptor descriptor, Lifestyle lifestyle = null)
            where TImplementation : class, ICommandProcessor
        {
            var producer = (lifestyle ?? _container.Options.DefaultLifestyle)
                .CreateProducer<ICommandProcessor, TImplementation>(_container);

            _producers.Add(descriptor, producer);
        }
    }
}