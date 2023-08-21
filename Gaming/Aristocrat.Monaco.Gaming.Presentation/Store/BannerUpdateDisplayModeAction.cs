namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using Aristocrat.Monaco.Gaming.Contracts.Lobby;

public record BannerUpdateDisplayModeAction
{
    public BannerDisplayMode Mode { get; init; }
}
