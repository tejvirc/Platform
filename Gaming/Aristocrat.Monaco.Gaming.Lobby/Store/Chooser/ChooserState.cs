﻿namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using System.Collections.Immutable;
using Fluxor;
using Models;

[FeatureState]
public record ChooserState
{
    public IImmutableList<ChooserItem> Items { get; set; }
}