namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using System.Collections.Immutable;
using Fluxor;
using Models;
using Services.Chooser;

[FeatureState]
public record ChooserState
{
    public IImmutableList<GameInfo> Games { get; init; }

    public ChooserStyle Style { get; init; }

    public bool IsExtraLargeIcons { get; set; }
}
