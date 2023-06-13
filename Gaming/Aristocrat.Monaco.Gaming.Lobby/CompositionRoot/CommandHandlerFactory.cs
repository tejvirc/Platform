namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using System;
using CommandHandlers;
using SimpleInjector;

/// <summary>
///     An implementation of <see cref="ICommandHandlerFactory" /> used to resolve a command handler from the container
/// </summary>
internal class CommandHandlerFactory : ICommandHandlerFactory
{
    private readonly Container _container;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandHandlerFactory" /> class.
    /// </summary>
    /// <param name="container">The container.</param>
    public CommandHandlerFactory(Container container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    /// <inheritdoc />
    public ICommandHandler<TCommand> Create<TCommand>()
    {
        return _container.GetInstance<ICommandHandler<TCommand>>();
    }
}
