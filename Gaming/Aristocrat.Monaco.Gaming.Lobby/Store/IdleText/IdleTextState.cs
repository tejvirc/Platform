namespace Aristocrat.Monaco.Gaming.Lobby.Store.IdleText;

using Contracts.Lobby;
using Fluxor;

[FeatureState]
public record IdleTextState
{
    public string? IdleText { get; set; }

    public bool IsScrollingEnabled { get; set; }

    public bool IsPaused { get; set; }

    public bool IsScrolling { get; set; }

    public BannerDisplayMode BannerDisplayMode { get; set; }
}
