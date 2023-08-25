namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record TranslateUpdateActiveLanguageAction
{
    public TranslateUpdateActiveLanguageAction(bool isPrimaryLanguageActive)
    {
        IsPrimaryLanguageActive = isPrimaryLanguageActive;
    }

    public bool IsPrimaryLanguageActive { get; }
}
