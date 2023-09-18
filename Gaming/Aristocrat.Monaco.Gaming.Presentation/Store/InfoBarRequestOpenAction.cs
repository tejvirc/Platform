namespace Aristocrat.Monaco.Gaming.Presentation.Store
{
    using Cabinet.Contracts;

    public record InfoBarRequestOpenAction
    {
        public InfoBarRequestOpenAction(DisplayRole displayTarget, bool open)
        {
            switch (displayTarget)
            {
                case DisplayRole.Main:
                    MainInfoBarOpenRequested = open;
                    break;
                case DisplayRole.VBD:
                    VbdInfoBarOpenRequested = open;
                    break;
            }
        }

        public bool MainInfoBarOpenRequested { get; }

        public bool VbdInfoBarOpenRequested { get; }
    }
}
