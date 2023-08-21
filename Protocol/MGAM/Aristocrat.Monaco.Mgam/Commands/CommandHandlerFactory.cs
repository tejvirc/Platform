namespace Aristocrat.Monaco.Mgam.Commands
{
    using System.Threading.Tasks;
    using SimpleInjector;

    /// <summary>
    ///     
    /// </summary>
    public class CommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly Container _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandHandlerFactory"/> class.
        /// </summary>
        /// <param name="container"></param>
        public CommandHandlerFactory(Container container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public async Task Execute<TCommand>(TCommand command)
            where TCommand : class
        {
            await _container.GetInstance<ICommandHandler<TCommand>>().Handle(command);
        }
    }
}
