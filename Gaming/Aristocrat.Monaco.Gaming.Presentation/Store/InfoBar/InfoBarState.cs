namespace Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;
public record InfoBarState
{
    public bool MainInfoBarOpenRequested { get; init; }

    public bool VbdInfoBarOpenRequested { get; init; }

    public bool MainInfoBarIsOpen { get; init; }

    public bool VbdInfoBarIsOpen { get; init; }

}
