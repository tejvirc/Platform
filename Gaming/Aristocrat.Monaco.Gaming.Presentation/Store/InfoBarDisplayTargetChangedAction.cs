namespace Aristocrat.Monaco.Gaming.Presentation.Store
{
    using Cabinet.Contracts;

    public record InfoBarDisplayTargetChangedAction
    {
        public InfoBarDisplayTargetChangedAction(DisplayRole displayTarget)
        {
            DisplayTarget = displayTarget;
        }

        public DisplayRole DisplayTarget;
    }
}
