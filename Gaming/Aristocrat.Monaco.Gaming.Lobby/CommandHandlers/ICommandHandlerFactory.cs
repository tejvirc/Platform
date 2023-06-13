namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

/// <summary>
///     Provides a mechanism to resolve a command handler for a command
/// </summary>
public interface ICommandHandlerFactory
{
    /// <summary>
    ///     Creates a command handler for the given command.
    /// </summary>
    /// <typeparam name="TCommand">The command</typeparam>
    /// <returns>The command handler</returns>
    ICommandHandler<TCommand> Create<TCommand>();
}
