namespace Aristocrat.Monaco.Gaming.Presentation.Store;
using System;
using Aristocrat.Cabinet.Contracts;
using Aristocrat.Monaco.Gaming.Contracts.InfoBar;

/// <summary>
///     Clears the messages from the specified InfoBar regions
/// </summary>
public record InfoBarClearMessageAction
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InfoBarClearMessageAction" /> record.
    /// </summary>
    /// <param name="ownerId">The owner Id.</param>
    /// <param name="displayTarget">The display target.</param>
    /// <param name="regions">The region to clear.</param>
    public InfoBarClearMessageAction(Guid ownerId, DisplayRole displayTarget, params InfoBarRegion[] regions)
    {
        OwnerId = ownerId;
        Regions = regions;
        DisplayTarget = displayTarget;
    }

    /// <summary>Gets the unique ID of the message owner.</summary>
    public Guid OwnerId { get; }

    /// <summary>Gets the regions to clear</summary>
    public InfoBarRegion[] Regions { get; }

    /// <summary>Gets the Display Target</summary>
    public DisplayRole DisplayTarget { get; }
}
