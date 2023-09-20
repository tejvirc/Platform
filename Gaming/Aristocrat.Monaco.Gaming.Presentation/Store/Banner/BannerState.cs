namespace Aristocrat.Monaco.Gaming.Presentation.Store.Banner;

/// <summary>
///     State of the Banner component, responsible for idle text
/// </summary>
public record BannerState
{
    public string? IdleTextFromCabinetOrHost { get; init; }

    public string? IdleTextFromJurisdiction { get; init; }

    public string? IdleTextDefault { get; init; }

    public bool IsPaused { get; init; }

    public bool IsScrolling { get; init; }

}