namespace Aristocrat.Monaco.Gaming.Lobby.Models;

using System.Collections.Generic;
using Contracts;

public class AttractVideoInfo
{
    private Dictionary<string, ILocaleGameGraphics> _localeAttractGraphics = new();

    public string? TopperAttractVideoPath { get; set; }

    public string? TopAttractVideoPath { get; set; }

    public string? BottomAttractVideoPath { get; set; }

    public string? GetTopAttractVideoPathByLocaleCode(string localeCode)
    {
        return _localeAttractGraphics.TryGetValue(localeCode, out var graphics) ? graphics.TopAttractVideo : null;
    }

    public string? GetTopperAttractVideoPathByLocaleCode(string localeCode)
    {
        return _localeAttractGraphics.TryGetValue(localeCode, out var graphics) ? graphics.TopperAttractVideo : null;
    }

    public string? GetBottomAttractVideoPathByLocaleCode(string localeCode)
    {
        return _localeAttractGraphics.TryGetValue(localeCode, out var graphics) ? graphics.BottomAttractVideo : null;
    }
}
