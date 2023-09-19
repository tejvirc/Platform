namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using Cabinet.Contracts;

/// <summary>
///     Set the height of the InfoBar
/// </summary>
public record InfoBarSetHeightAction
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InfoBarSetHeightAction" /> record.
    /// </summary>
    /// <param name="height">The height.</param>
    /// <param name="displayTarget">The display which this event is targeted at</param>
    public InfoBarSetHeightAction(double height, DisplayRole displayTarget)
    {
        Height = height;
    }

    /// <summary>Gets the height</summary>
    public double Height { get; }
}