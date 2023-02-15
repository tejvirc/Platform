namespace Aristocrat.Monaco.Bingo.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleInjector;

    /// <summary>
    ///     Command handler factory
    /// </summary>
    public class CommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly Container _container;

        public CommandHandlerFactory(Container container)
        {
            _container = container;
        }

        public async Task Execute<TCommand>(TCommand command, CancellationToken token = default)
            where TCommand : class
        {
            await _container.GetInstance<ICommandHandler<TCommand>>().Handle(command, token);
        }
    }
}