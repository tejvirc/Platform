namespace Aristocrat.Monaco.UI.Common;

using System.Windows.Forms.Integration;

/// <summary>
///     Extending WindowsFormhost
/// </summary>
public class ExtendedWindowsFormsHost : WindowsFormsHost
{
    /*
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedWindowsFormsHost"/> class
    /// </summary>
    public ExtendedWindowsFormsHost()
    {
        ChildChanged += OnChildChanged;
    }

    private void OnChildChanged(object sender, ChildChangedEventArgs childChangedEventArgs)
    {
        var previousChild = childChangedEventArgs.PreviousChild as Control;
        if (previousChild != null)
        {
            previousChild.MouseDown -= OnMouseDown;
        }

        if (Child != null)
        {
            Child.MouseDown += OnMouseDown;
        }
    }

    private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs mouseEventArgs)
    {
        MouseButton? wpfButton = ConvertToWpf(mouseEventArgs.Button);
        if (!wpfButton.HasValue)
        {
            return;
        }

        RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, wpfButton.Value)
        {
            RoutedEvent = Mouse.MouseDownEvent,
            Source = this,
        });
    }

    private MouseButton? ConvertToWpf(MouseButtons winformButton)
    {
        switch (winformButton)
        {
            case MouseButtons.Left:
                return MouseButton.Left;
            case MouseButtons.None:
                return null;
            case MouseButtons.Right:
                return MouseButton.Right;
            case MouseButtons.Middle:
                return MouseButton.Middle;
            case MouseButtons.XButton1:
                return MouseButton.XButton1;
            case MouseButtons.XButton2:
                return MouseButton.XButton2;
            default:
                throw new ArgumentOutOfRangeException("winformButton");
        }
    }
    */
}
