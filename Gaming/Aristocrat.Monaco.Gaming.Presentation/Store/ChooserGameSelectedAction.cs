namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using UI.Models;

public record ChooserGameSelectedAction
{
    public ChooserGameSelectedAction(GameInfo game)
    {
        Game = game;
    }

    public GameInfo Game { get; }
}
