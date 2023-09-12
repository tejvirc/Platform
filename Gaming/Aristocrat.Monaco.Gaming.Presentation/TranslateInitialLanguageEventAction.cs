namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record TranslateInitialLanguageEventAction
{
    public TranslateInitialLanguageEventAction(bool isInitialLanguageEventSent)
    {
        IsInitialLanguageEventSent = isInitialLanguageEventSent;
    }

    public bool IsInitialLanguageEventSent { get; }
}