namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record TranslateUpdatePrimaryLanguageAction
{
    public TranslateUpdatePrimaryLanguageAction(bool isPrimaryLanguageActive)
    {
        IsPrimaryLanguageActive = isPrimaryLanguageActive;
    }

    public bool IsPrimaryLanguageActive { get; }
}