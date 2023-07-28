namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record UpdatePrimaryLanguageSelectedAction
{
    public UpdatePrimaryLanguageSelectedAction(bool isSelected)
    {
        IsSelected = isSelected;
    }

    public bool IsSelected { get; }
}
