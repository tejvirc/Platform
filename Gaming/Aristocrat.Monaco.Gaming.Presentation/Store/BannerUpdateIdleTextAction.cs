namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public enum IdleTextType
{
    CabinetOrHost,
    Jurisdiction,
    Default
}

/// <summary>
///     Change banner idle text
/// </summary>
public record BannerUpdateIdleTextAction
{
    public BannerUpdateIdleTextAction(IdleTextType textType, string? text)
    {
        TextType = textType;
        IdleText = text;
    }

    public IdleTextType TextType { get; }
    public string? IdleText { get; }
}
