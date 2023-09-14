namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System;

/// <summary>
///     Change banner idle text
/// </summary>
public record BannerUpdateIdleTextAction
{
    public BannerUpdateIdleTextAction(string text)
    {
        IdleText = text;
    }

    public string IdleText { get; }
}
