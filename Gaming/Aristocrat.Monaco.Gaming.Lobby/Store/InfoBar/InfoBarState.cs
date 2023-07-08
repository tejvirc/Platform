﻿namespace Aristocrat.Monaco.Gaming.Lobby.Store.InfoBar;

using System.Collections.Immutable;
using Fluxor;
using Models;

[FeatureState]
public record InfoBarState
{
    public ImmutableList<InfoOverlayText>? InfoOverlayTextItems { get; set; }
}
