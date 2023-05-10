namespace Aristocrat.Monaco.Gaming.Lobby.Behaviors;

using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using XAMLMarkupExtensions.Base;

public class GameWindowBehavior : Behavior<Window>
{
    /// <summary>
    ///     <see cref="DependencyProperty"/> set a value that determines if the target element will be aware of localization changes.
    /// </summary>
    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.RegisterAttached(
            "IsActive",
            typeof(bool),
            typeof(GameWindowBehavior),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.None, OnIsActiveChanged));

    private static void OnIsActiveChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        if (target is not GameWindowBehavior behavior)
        {
            return;
        }

        behavior.AssociatedObject.Visibility = (bool)args.NewValue ? Visibility.Visible : Visibility.Hidden;
        behavior.AssociatedObject.Activate();
    }

    /// <summary>
    ///     Gets or sets the <see cref="IsActiveProperty"/> value.
    /// </summary>
    public string IsActive
    {
        get => (string)GetValue(IsActiveProperty);

        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    ///     Getter of <see cref="IsActiveProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <returns>The value of the property.</returns>
    public static string GetIsActive(DependencyObject obj)
    {
        return obj.GetValueSync<string>(IsActiveProperty);
    }

    /// <summary>
    ///     Setter of <see cref="IsActiveProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetIsActive(DependencyObject obj, string value)
    {
        obj.SetValueSync(IsActiveProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject?.Owner is not { } parent)
        {
            return;
        }

        AssociatedObject.Top = parent.Top;
        AssociatedObject.Left = parent.Left;
        AssociatedObject.Width = parent.ActualWidth;
        AssociatedObject.Height = parent.ActualHeight;

        parent.SizeChanged += OnParentSizeChanged;
        parent.LocationChanged += OnParentLocationChanged;
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject?.Owner is not { } parent)
        {
            return;
        }

        parent.SizeChanged -= OnParentSizeChanged;
        parent.LocationChanged -= OnParentLocationChanged;
    }

    private void OnParentLocationChanged(object? sender, EventArgs e)
    {
        if (sender is not Window platform)
        {
            return;
        }

        AssociatedObject.Top = platform.Top;
        AssociatedObject.Left = platform.Left;
    }

    private void OnParentSizeChanged(object sender, SizeChangedEventArgs e)
    {
        AssociatedObject.Width = e.NewSize.Width;
        AssociatedObject.Height = e.NewSize.Height;
    }
}
