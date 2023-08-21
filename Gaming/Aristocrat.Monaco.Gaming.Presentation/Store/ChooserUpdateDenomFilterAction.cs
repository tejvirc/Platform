namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record ChooserUpdateDenomFilterAction
{
    public ChooserUpdateDenomFilterAction(int denomFilter)
    {
        DenomFilter = denomFilter;
    }

    public int DenomFilter { get; }
}
