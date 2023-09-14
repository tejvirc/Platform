namespace Aristocrat.Monaco.Gaming.Presentation.Store.Banner;

public record BannerState
{
    public string? IdleText { get; init; }

    public bool IsIdleTextShowing { get; init; }

    public bool IsPaused { get; init; }

    public bool IsScrolling { get; init; }

}