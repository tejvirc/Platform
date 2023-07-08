namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using System;
using CommandHandlers;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     An implementation of <see cref="ICommandHandlerFactory" /> used to resolve a command handler from the container
/// </summary>
internal class CommandHandlerFactory : ICommandHandlerFactory, IDisposable
{
    private readonly IServiceScope _scope;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandHandlerFactory" /> class.
    /// </summary>
    /// <param name="container">The container.</param>
    public CommandHandlerFactory(IServiceScopeFactory serviceScopeFactory)
    {
        if (serviceScopeFactory == null)
        {
            throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        _scope = serviceScopeFactory.CreateScope();

        var commandHandlers = _scope.ServiceProvider.GetServices(typeof(ICommandHandler<>));
    }

    /// <inheritdoc />
    public ICommandHandler<TCommand> Create<TCommand>()
    {
        return _scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
