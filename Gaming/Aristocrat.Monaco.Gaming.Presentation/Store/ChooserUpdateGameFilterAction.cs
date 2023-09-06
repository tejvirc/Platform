namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using Gaming.Contracts.Models;

public record ChooserUpdateGameFilterAction
{
    public ChooserUpdateGameFilterAction(GameType filter)
    {
        Filter = filter;
    }

    public GameType Filter { get; }
}
