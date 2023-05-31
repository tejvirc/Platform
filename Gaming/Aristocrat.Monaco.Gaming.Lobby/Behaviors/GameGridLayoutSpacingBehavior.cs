namespace Aristocrat.Monaco.Gaming.Lobby.Behaviors;

using System.Windows;
using Controls;
using Microsoft.Xaml.Behaviors;
using XAMLMarkupExtensions.Base;

public class GameGridLayoutSpacingBehavior : Behavior<GameLayoutPanel>
{
    /// <summary>
    ///     <see cref="DependencyProperty"/> set a value that determines if the target element will be aware of localization changes.
    /// </summary>
    public static readonly DependencyProperty GameCountProperty =
        DependencyProperty.RegisterAttached(
            "GameCount",
            typeof(int),
            typeof(GameGridLayoutSpacingBehavior),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnGameCountChanged));

    private static void OnGameCountChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        if (target is not GameGridLayoutSpacingBehavior behavior)
        {
            return;
        }

        var gameCount = (int)args.NewValue;

        behavior.AssociatedObject.Spacing = gameCount <= 8
            ? new Size(40, 50)
            : gameCount == 9
                ? new Size(130, 18)
                : gameCount <= 15
                    ? new Size(40, 20)
                    : gameCount <= 18
                        ? new Size(20, 50)
                        : gameCount <= 21
                            ? new Size(20, 80)
                            : gameCount <= 36
                                ? new Size(20, 50)
                                : new Size(20, 80);
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
