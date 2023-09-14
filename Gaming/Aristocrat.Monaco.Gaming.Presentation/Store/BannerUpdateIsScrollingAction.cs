namespace Aristocrat.Monaco.Gaming.Presentation.Store;

/// <summary>
///     Update state to reflect if idle text is actively scrolling or has completed,
///     to ensure all text has time to be read
/// </summary>
public record BannerUpdateIsScrollingAction
{
    public BannerUpdateIsScrollingAction(bool isScrolling)
    {
        IsScrolling = isScrolling;
    }

    public bool IsScrolling { get; }
}