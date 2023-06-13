namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

/// <summary>
///     Interface for a command handler.
/// </summary>
/// <typeparam name="TCommand">Type of the command.</typeparam>
public interface ICommandHandler<in TCommand>
{
    /// <summary>
    ///     Handles/processes the given command.
    /// </summary>
    /// <param name="command">The command.</param>
    void Handle(TCommand command);
}
