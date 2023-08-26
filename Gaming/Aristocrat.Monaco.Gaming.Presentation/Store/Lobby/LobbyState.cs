namespace Aristocrat.Monaco.Gaming.Presentation.Store.Lobby;

using System;
using Gaming.Contracts.Models;

public record LobbyState
{
    public string? BackgroundImagePath { get; init; }

    public DateTime LastUserInteractionTime { get; init; }

    public bool IsVoucherNotificationActive { get; init; }

    public bool IsProgressiveGameDisabledNotificationActive { get; init; }

    public bool IsStartingUp { get; init; }

    public bool IsInitialized { get; init; }
}
