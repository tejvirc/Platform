namespace Aristocrat.Monaco.Gaming.Lobby.Commands;

using Extensions.Prism.Commands;

public class ApplicationCommands : IApplicationCommands
{
    public CompositeCommand ShutdownCommand => ApplicationCommandConstants.ShutdownCommand;
}
