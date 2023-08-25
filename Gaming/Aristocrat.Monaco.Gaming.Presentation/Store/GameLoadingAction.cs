namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using UI.Models;

public record GameLoadingAction
{
    public GameLoadingAction(GameInfo game)
    {
        Game = game;
    }

    public GameInfo Game { get; }
}
