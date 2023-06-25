namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using Aristocrat.Monaco.Gaming.Contracts.Lobby;

public record UpdateBannerDisplayModeAction
{
    public BannerDisplayMode Mode { get; init; }
}
