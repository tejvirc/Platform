namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using Fluxor;

[FeatureState]
public record ChooserState
{
    public bool IsExtraLargeIcons { get; set; }

    public int GamesPerPage { get; set; }
}
