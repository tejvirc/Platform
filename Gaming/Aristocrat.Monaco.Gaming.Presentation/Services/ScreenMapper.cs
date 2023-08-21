namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System.Windows;
using Aristocrat.Monaco.Application.UI.Views;
using Cabinet.Contracts;
using Common;
using Kernel;

public class ScreenMapper : IScreenMapper
{
    private readonly IPropertiesManager _properties;

    public ScreenMapper(IPropertiesManager properties)
    {
        _properties = properties;
    }

    public ScreenMapResult Map(DisplayRole role, Window window, bool dryRun = false, ScreenMapOptions? options = null)
    {
        if (!dryRun)
        {
            new WindowToScreenMapper(role).MapWindow(window);
        }

        var result = new ScreenMapResult
        {
            Role = role,
            IsFullscreen = GetFullscreen(),
            ShowCursor = GetShowCursor()
        };

        return result;
    }

    private bool GetFullscreen()
    {
        var display = _properties.GetValue(Constants.DisplayPropertyKey, Constants.DisplayPropertyFullScreen);

        display = display.ToUpperInvariant();

        return display == Constants.DisplayPropertyFullScreen;
    }

    private bool GetShowCursor()
    {
        var showMouseCursor = _properties.GetValue(Constants.ShowMouseCursor, Constants.False);

        showMouseCursor = showMouseCursor.ToUpperInvariant();

        return showMouseCursor == Constants.True;
    }
}
