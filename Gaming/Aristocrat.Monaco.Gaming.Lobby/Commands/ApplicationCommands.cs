namespace Aristocrat.Monaco.Gaming.Lobby.Commands;

using Toolkit.Mvvm.Extensions.Commands;

public class ApplicationCommands : IApplicationCommands
{
    public CompositeCommand ShutdownCommand => GlobalApplicationCommands.ShutdownCommand;
}
