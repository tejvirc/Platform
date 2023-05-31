namespace Aristocrat.Monaco.Gaming.Lobby.Behaviors;

using System.Windows;
using Microsoft.Xaml.Behaviors;
using XAMLMarkupExtensions.Base;

public class GameGridMarginBehavior : Behavior<FrameworkElement>
{
    /// <summary>
    ///     <see cref="DependencyProperty"/> set a value that determines if the target element will be aware of localization changes.
    /// </summary>
    public static readonly DependencyProperty GameCountProperty =
        DependencyProperty.RegisterAttached(
            "GameCount",
            typeof(int),
            typeof(GameGridMarginBehavior),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnGameCountChanged));

    private static void OnGameCountChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        if (target is not GameGridMarginBehavior behavior)
        {
            return;
        }

        var useSmallIcons = (int)args.NewValue > 8;

        behavior.AssociatedObject.Margin = useSmallIcons
            ? new Thickness(0.0, 50.0, 0.0, 0)
            : new Thickness(0.0, 80.0, 0.0, 0);
    }

    /// <summary>
    ///     Gets or sets the <see cref="GameCountProperty"/> value.
    /// </summary>
    public int GameCount
    {
        get => (int)GetValue(GameCountProperty);

        set => SetValue(GameCountProperty, value);
    }

    /// <summary>
    ///     Getter of <see cref="GameCountProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <returns>The value of the property.</returns>
    public static int GetGameCount(DependencyObject obj)
    {
        return obj.GetValueSync<int>(GameCountProperty);
    }

    /// <summary>
    ///     Setter of <see cref="GameCountProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetGameCount(DependencyObject obj, int value)
    {
        obj.SetValueSync(GameCountProperty, value);
    }
}
