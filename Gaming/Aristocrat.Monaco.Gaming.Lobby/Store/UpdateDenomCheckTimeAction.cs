namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System;

public record UpdateDenomCheckTimeAction
{
    public DateTime CheckTime { get; init; }
}
