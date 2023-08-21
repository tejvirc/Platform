﻿namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using Aristocrat.Cabinet.Contracts;

public class ScreenMapResult
{
    public DisplayRole Role { get; init; }

    public bool IsFullscreen { get; init; }

    public bool IsWindowed => !IsFullscreen;

    public bool ShowCursor { get; init; }
}
