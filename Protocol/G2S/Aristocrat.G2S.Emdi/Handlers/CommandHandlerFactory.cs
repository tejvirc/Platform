namespace Aristocrat.G2S.Emdi.Handlers
{
    using System;
    using Emdi.Host;
    using Protocol.v21ext1b1;
    using SimpleInjector;

    /// <summary>
    /// Creates instance of command handler
    /// </summary>
    public class CommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly Container _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerFactory"/> class.
        /// </summary>
        /// <param name="container">Instance of container</param>
        public CommandHandlerFactory(Container container)
        {
            _container = container;
        }

        /// <param name="context"></param>
        /// <inheritdoc />
        public ICommandHandler<TCommand> Create<TCommand>(RequestContext context)
            where TCommand : c_baseCommand
        {
            var handler = _container.GetInstance<ICommandHandler<TCommand>>();

            handler.Context = context ?? throw new ArgumentNullException(nameof(context));

            return handler;
        }
    }
}
