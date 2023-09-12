namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record TranslateUpdateMultiLanguageAction
{
    public TranslateUpdateMultiLanguageAction(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public bool IsEnabled { get; }
}