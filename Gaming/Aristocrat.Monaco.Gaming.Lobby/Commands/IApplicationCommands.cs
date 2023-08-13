namespace Aristocrat.Monaco.Gaming.Lobby.Commands;

using Aristocrat.MVVM.Command;
// using Extensions.Prism.Commands;

public interface IApplicationCommands
{
    CompositeCommand ShutdownCommand { get; }
}
