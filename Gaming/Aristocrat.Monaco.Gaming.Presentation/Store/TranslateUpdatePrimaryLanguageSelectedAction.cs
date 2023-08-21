namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record UpdateActiveLanguageAction
{
    public UpdateActiveLanguageAction(bool isPrimaryLanguageActive)
    {
        IsPrimaryLanguageActive = isPrimaryLanguageActive;
    }

    public bool IsPrimaryLanguageActive { get; }
}
