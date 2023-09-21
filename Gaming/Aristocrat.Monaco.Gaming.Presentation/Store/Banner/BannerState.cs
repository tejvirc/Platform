namespace Aristocrat.Monaco.Gaming.Presentation.Store.Banner;

/// <summary>
///     State of the Banner component, responsible for idle text
/// </summary>
public record BannerState
{
    /// <summary>
    ///     Current idle text to display
    /// </summary>
    public string? CurrentIdleText { get; init; }

    /// <summary>
    ///     Whether or not idle text animation is paused due to platform being disabled
    /// </summary>
    public bool IsPaused { get; init; }

    /// <summary>
    ///     Whether or not idle text is both in scrolling mode and actively scrolling
    /// </summary>
    public bool IsScrolling { get; init; }
}