namespace Aristocrat.Monaco.Gaming.Presentation.Commands;

using Aristocrat.MVVM.Command;
// using Extensions.Prism.Commands;

public class ApplicationCommands : IApplicationCommands
{
    public CompositeCommand ShutdownCommand => ApplicationCommandConstants.ShutdownCommand;
}
