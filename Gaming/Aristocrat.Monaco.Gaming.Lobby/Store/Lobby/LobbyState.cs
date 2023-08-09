namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System;
using System.Collections.Immutable;
using Aristocrat.Monaco.Application.Contracts.EdgeLight;
using Contracts.Lobby;
using Contracts.Models;
using Fluxor;
using UI.Models;

public record LobbyState
{
    public bool IsGamesLoaded { get; set; }

    public string? BackgroundImagePath { get; set; }

    public DateTime LastUserInteractionTime { get; set; }

    public double Credits { get; set; }

    public bool IsCashingOut { get; set; }

    public LobbyCashOutState CurrentCashOutState { get; set; }

    public bool IsVoucherNotificationActive { get; set; }

    public bool IsProgressiveGameDisabledNotificationActive { get; set; }

    public bool IsStartingUp { get; init; }

    public bool IsInitialized { get; init; }
}
