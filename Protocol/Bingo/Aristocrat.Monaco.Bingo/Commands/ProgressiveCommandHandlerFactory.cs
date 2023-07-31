namespace Aristocrat.Monaco.Bingo.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleInjector;

    /// <summary>
    ///     Progressive command handler factory
    /// </summary>
    public class ProgressiveCommandHandlerFactory : IProgressiveCommandHandlerFactory
    {
        private readonly Container _container;

        public ProgressiveCommandHandlerFactory(Container container)
        {
            _container = container;
        }

        public async Task Execute<TCommand>(TCommand command, CancellationToken token = default)
            where TCommand : class
        {
            await _container.GetInstance<IProgressiveCommandHandler<TCommand>>().Handle(command, token);
        }
    }
}