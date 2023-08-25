namespace Aristocrat.Monaco.Gaming.Presentation;

using System.Windows;
using System.Windows.Input;

public static class WindowExtensions
{
    public static TWindow ShowWithTouch<TWindow>(this TWindow window) where TWindow : Window
    {
        window.Show();
        window.Activate();
        window.SetStylusSettings();

        return window;
    }

    public static Window SetStylusSettings(this Window window)
    {
        Stylus.SetIsTapFeedbackEnabled(window, false);
        Stylus.SetIsTouchFeedbackEnabled(window, false);
        Stylus.SetIsPressAndHoldEnabled(window, false);
        Stylus.SetIsFlicksEnabled(window, false);

        return window;
    }
}
