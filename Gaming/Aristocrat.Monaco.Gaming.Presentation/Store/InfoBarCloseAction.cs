namespace Aristocrat.Monaco.Gaming.Presentation.Store;
using Aristocrat.Cabinet.Contracts;

/// <summary>
///     Signals the InfoBar to close
/// </summary>
public record InfoBarCloseAction
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InfoBarCloseAction" /> record.
    /// </summary>
    /// <param name="displayTarget">The display target.</param>
    public InfoBarCloseAction(DisplayRole displayTarget)
    {
    }
}
