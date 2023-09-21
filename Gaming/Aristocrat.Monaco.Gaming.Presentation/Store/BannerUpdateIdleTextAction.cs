namespace Aristocrat.Monaco.Gaming.Presentation.Store;

/// <summary>
///     Change banner idle text
/// </summary>
public record BannerUpdateIdleTextAction
{
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="text"></param>
    public BannerUpdateIdleTextAction(string? text)
    {
        IdleText = text;
    }

    /// <summary>
    ///     String to set for idle text
    /// </summary>
    public string? IdleText { get; }
}