namespace Aristocrat.Monaco.Gaming.Presentation.Store.Game;

using System;
using UI.Models;

public record GameState
{
    public GameInfo? Selected { get; init; }

    public IntPtr BottomWindowHandle { get; init; }

    public IntPtr TopWindowHandle { get; init; }

    public IntPtr TopperWindowHandle { get; init; }

    public IntPtr ButtonDeckWindowHandle { get; init; }
}
