namespace Aristocrat.Monaco.Gaming.Lobby.Regions;

using System.Windows;
using XAMLMarkupExtensions.Base;

public class RegionManager : DependencyObject
{
    /// <summary>
    ///     <see cref="DependencyProperty"/> set the target region.
    /// </summary>
    public static readonly DependencyProperty RegionProperty =
        DependencyProperty.RegisterAttached(
            "Region",
            typeof(string),
            typeof(RegionManager),
            new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.None));

    /// <summary>
    ///     Gets or sets the <see cref="RegionProperty"/> value.
    /// </summary>
    public string Region
    {
        get => (string)GetValue(RegionProperty);

        set => SetValue(RegionProperty, value);
    }

    /// <summary>
    ///     Getter of <see cref="RegionProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <returns>The value of the property.</returns>
    public static string GetRegion(DependencyObject obj)
    {
        return obj?.GetValueSync<string>(RegionProperty);
    }

    /// <summary>
    ///     Setter of <see cref="RegionProperty"/>.
    /// </summary>
    /// <param name="obj">The target dependency object.</param>
    /// <param name="value">The value of the property.</param>
    public static void SetRegion(DependencyObject obj, string value)
    {
        obj?.SetValueSync(RegionProperty, value);
    }
}
