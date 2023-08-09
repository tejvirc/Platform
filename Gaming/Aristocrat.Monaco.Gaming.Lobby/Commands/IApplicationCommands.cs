namespace Aristocrat.Monaco.Gaming.Lobby.Commands;

using Extensions.Prism.Commands;

public interface IApplicationCommands
{
    CompositeCommand ShutdownCommand { get; }
}
