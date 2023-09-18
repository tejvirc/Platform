namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using Cabinet.Contracts;

public record InfoBarSetHeightAction
{
    public InfoBarSetHeightAction(double height, DisplayRole displayTarget)
    {
        Height = height;
    }

    public double Height { get; }
}