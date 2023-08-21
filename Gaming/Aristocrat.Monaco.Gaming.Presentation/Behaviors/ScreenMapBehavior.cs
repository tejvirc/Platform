namespace Aristocrat.Monaco.Gaming.Presentation.Behaviors;

using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using Prism.Ioc;
using Services;
using XAMLMarkupExtensions.Base;
using DisplayRoleEnum = Cabinet.Contracts.DisplayRole;

public class ScreenMapBehavior : Behavior<Window>
{
    /// <summary>
    ///     <see cref="DependencyProperty"/> set a value that determines if the target element will be aware of localization changes.
    /// </summary>
    public static readonly DependencyProperty DisplayRoleProperty =
        DependencyProperty.RegisterAttached(
            "DisplayRole",
            typeof(DisplayRoleEnum),
            typeof(ScreenMapBehavior),
            new FrameworkPropertyMetadata(DisplayRoleEnum.Unknown,
                FrameworkPropertyMetadataOptions.None, OnDisplayRoleChanged));

    /// <summary>
    ///     <see cref="DependencyProperty"/> set a value that determines if the target element will be aware of localization changes.
    /// </summary>
    public static readonly DependencyProperty IsFullscreenProperty =
        DependencyProperty.RegisterAttached(
            "IsFullscreen",
            typeof(bool),
            typeof(ScreenMapBehavior),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.Inherits |
                FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior));

    /// <summary>
    ///     Gets or sets the <see cref="DisplayRoleProperty"/> value.
    /// </summary>
    public DisplayRoleEnum DisplayRole
    {
        get => (DisplayRoleEnum)GetValue(DisplayRoleProperty);

        set => SetValue(DisplayRoleProperty, value);
    }

    /// <summary>
    ///     Gets or sets the <see cref="IsFullscreenProperty"/> value.
    /// </summary>
    public bool IsFullscreen
    {
        get => (bool)GetValue(IsFullscreenProperty);

        set => SetValue(IsFullscreenProperty, value);
    }

    /// <summary>
    ///     Getter of <see cref="DisplayRoleProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <returns>The value of the property.</returns>
    public static string GetDisplayRole(DependencyObject obj)
    {
        return obj.GetValueSync<string>(DisplayRoleProperty);
    }

    /// <summary>
    ///     Setter of <see cref="DisplayRoleProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetDisplayRole(DependencyObject obj, string value)
    {
        obj.SetValueSync(DisplayRoleProperty, value);
    }

    /// <summary>
    ///     Getter of <see cref="IsFullscreenProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <returns>The value of the property.</returns>
    public static string GetIsFullscreen(DependencyObject obj)
    {
        return obj.GetValueSync<string>(IsFullscreenProperty);
    }

    /// <summary>
    ///     Setter of <see cref="IsFullscreenProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetIsFullscreen(DependencyObject obj, string value)
    {
        obj.SetValueSync(IsFullscreenProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.Loaded += OnLoaded;
    }

    private static void OnDisplayRoleChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        var displayRole = (DisplayRoleEnum)args.NewValue;
        if (displayRole == DisplayRoleEnum.Unknown)
        {
            throw new ArgumentException(@"Display role cannot be unknown", nameof(args));
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DisplayRole == DisplayRoleEnum.Unknown)
        {
            throw new InvalidOperationException($"{nameof(DisplayRole)} property not set");
        }

        var result = ContainerLocator.Current.Resolve<IScreenMapper>()
            .Map(DisplayRole, AssociatedObject);

        IsFullscreen = result.IsFullscreen;
    }
}
