namespace Aristocrat.Monaco.Gaming.Lobby.Models;

using Aristocrat.Monaco.Gaming.Contracts.Models;
using System;
using CommunityToolkit.Mvvm.ComponentModel;

public class InfoOverlayText : ObservableObject
{
    public InfoOverlayText()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    ///     Gets the text ID
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Gets or sets text to set
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    ///     Gets or sets the location to send the text
    /// </summary>
    public InfoLocation Location { get; init; }
}
