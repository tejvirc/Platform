namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using System;
using System.Collections.Immutable;
using Contracts.Models;
using Fluxor;
using Models;

[FeatureState]
public record ChooserState
{
    public IImmutableList<GameInfo> Games { get; set; } = ImmutableList<GameInfo>.Empty;

    public int UniqueThemesCount { get; set; }

    public bool AllowGameInCharge { get; set; }

    public bool IsSingleGame { get; set; }

    public bool IsTabView { get; set; }

    public int DenomFilter { get; set; }

    public GameType GameFilter { get; set; }

    public DateTime DenomCheckTime { get; set; }
}
