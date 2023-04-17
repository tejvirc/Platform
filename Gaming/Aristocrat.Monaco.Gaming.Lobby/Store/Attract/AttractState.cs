namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attract;

using System.Collections.Immutable;
using Contracts;
using Fluxor;

[FeatureState]
public record AttractState
{
    public ImmutableList<IAttractInfo> AttractItems { get; init; } = ImmutableList<IAttractInfo>.Empty;

    public int CurrentAttractIndex { get; init; } = 0;
}
