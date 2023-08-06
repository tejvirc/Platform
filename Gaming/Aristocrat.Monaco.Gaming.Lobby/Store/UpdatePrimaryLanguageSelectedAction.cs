namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record UpdateActiveLanguageAction
{
    public UpdateActiveLanguageAction(bool isPrimaryLanguageActive)
    {
        IsPrimaryLanguageActive = isPrimaryLanguageActive;
    }

    public bool IsPrimaryLanguageActive { get; }
}
