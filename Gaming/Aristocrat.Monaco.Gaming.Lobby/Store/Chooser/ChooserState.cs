namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using System.Collections.Immutable;
using Fluxor;
using Models;

[FeatureState]
public record ChooserState
{
    public IImmutableList<GameInfo> Games { get; set; } = ImmutableList<GameInfo>.Empty;

    public bool IsExtraLargeIcons { get; set; }

    public int GamesPerPage { get; set; }
}
