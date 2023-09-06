namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public class TranslateUpdateMultiLanguageAction
{
    public TranslateUpdateMultiLanguageAction(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public bool IsEnabled { get; }
}
