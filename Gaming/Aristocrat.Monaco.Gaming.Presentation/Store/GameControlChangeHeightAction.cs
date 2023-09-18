namespace Aristocrat.Monaco.Gaming.Presentation.Store;
public record GameControlChangeHeightAction
{
    public GameControlChangeHeightAction(double height)
    {
        GameControlHeight = height;
    }

    public double GameControlHeight { get; }
}
