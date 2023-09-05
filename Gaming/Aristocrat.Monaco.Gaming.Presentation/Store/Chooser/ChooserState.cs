namespace Aristocrat.Monaco.Gaming.Presentation.Store.Chooser;

using System;
using System.Collections.Immutable;
using Gaming.Contracts.Models;
using UI.Models;

public record ChooserState
{
    public IImmutableList<GameInfo> Games { get; init; }

    public bool AllowGameInCharge { get; init; }

    public bool IsTabView { get; init; }

    public int DenomFilter { get; init; }

    public GameType GameFilter { get; init; }

    public DateTime DenomCheckTime { get; init; }
}
