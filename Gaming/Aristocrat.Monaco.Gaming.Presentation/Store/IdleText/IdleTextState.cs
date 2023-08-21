namespace Aristocrat.Monaco.Gaming.Presentation.Store.IdleText;

using Contracts.Lobby;

public record IdleTextState
{
    public string? IdleText { get; init; }

    public bool IsScrollingEnabled { get; init; }

    public bool IsPaused { get; init; }

    public bool IsScrolling { get; init; }

    public BannerDisplayMode BannerDisplayMode { get; init; }
}
