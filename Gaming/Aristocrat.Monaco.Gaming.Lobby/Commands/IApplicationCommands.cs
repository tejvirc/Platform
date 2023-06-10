namespace Aristocrat.Monaco.Gaming.Lobby.Commands;

using Toolkit.Mvvm.Extensions.Commands;

public interface IApplicationCommands
{
    CompositeCommand ShutdownCommand { get; }
}
