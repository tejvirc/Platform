namespace Aristocrat.Monaco.Gaming.Presentation.Commands;

using Aristocrat.MVVM.Command;
// using Extensions.Prism.Commands;

public interface IApplicationCommands
{
    CompositeCommand ShutdownCommand { get; }
}
