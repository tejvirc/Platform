namespace Aristocrat.Monaco.Gaming.Presentation.Store.Game;

using System;
using UI.Models;

public record GameState
{
    public GameInfo? SelectedGame { get; init; }

    public IntPtr MainWindowHandle { get; init; }

    public IntPtr TopWindowHandle { get; init; }

    public IntPtr TopperWindowHandle { get; init; }

    public IntPtr ButtonDeckWindowHandle { get; init; }

    public bool IsLoaded { get; init; }

    public bool IsLoading { get; init; }
}
