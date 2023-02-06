namespace Aristocrat.Monaco.UI.Common.Controls.Helpers
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using MahApps.Metro.Controls;

    /// <summary>
    ///     Control helper class for <see cref="ToggleSwitchButton" />
    /// </summary>
    public static class ToggleSwitchButtonHelper
    {
        /// <summary>
        ///     Dependency property for setting the Off content />
        /// </summary>
        public static readonly DependencyProperty OffContentProperty =
            DependencyProperty.RegisterAttached(
                "OffContent",
                typeof(object),
                typeof(ToggleSwitchButtonHelper),
                new FrameworkPropertyMetadata(
                    "OFF",
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                    OnOffContentChanged));

        /// <summary>
        ///     Dependency property for setting the On content of the <see cref="ToggleSwitchButtonHelper" />
        /// </summary>
        public static readonly DependencyProperty OnContentProperty =
            DependencyProperty.RegisterAttached(
                "OnContent",
                typeof(object),
                typeof(ToggleSwitchButtonHelper),
                new FrameworkPropertyMetadata(
                    "ON",
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                    OnOnContentChanged));

        /// <summary>
        ///     Dependency property for setting the Off foreground color of the <see cref="ToggleSwitchButton" />
        /// </summary>
        public static readonly DependencyProperty OffBrushProperty =
            DependencyProperty.RegisterAttached(
                "OffBrush",
                typeof(Brush),
                typeof(ToggleSwitchButtonHelper),
                new FrameworkPropertyMetadata(
                    new SolidColorBrush(Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa)),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnOffBrushChanged));

        /// <summary>
        ///     Dependency property for setting the On foreground color of the <see cref="ToggleSwitchButton" />
        /// </summary>
        public static readonly DependencyProperty OnBrushProperty =
            DependencyProperty.RegisterAttached(
                "OnBrush",
                typeof(Brush),
                typeof(ToggleSwitchButtonHelper),
                new FrameworkPropertyMetadata(
                    Brushes.White,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnOnBrushChanged));

        /// <summary>
        ///     Gets the Off content for the <see cref="ToggleSwitchButton" />
        /// </summary>
        /// <param name="element"></param>
        /// <returns>The content</returns>
        [AttachedPropertyBrowsableForType(typeof(ToggleSwitch))]
        public static object GetOffContent(UIElement element)
        {
            return element.GetValue(OffContentProperty);
        }

        /// <summary>
        ///     Sets the Off content for the <see cref="ToggleSwitchButton" />
        /// </summary>
        /// <param name="element"></param>
        /// <param name="content">The content</param>
        public static void SetOffContent(UIElement element, object content)
        {
            element.SetValue(OffContentProperty, content);
        }

        /// <summary>
        ///     Gets the On content for the <see cref="ToggleSwitchButton" />
        /// </summary>
        /// <param name="element"></param>
        /// <returns>The content</returns>
        public static object GetOnContent(UIElement element)
        {
            return element.GetValue(OnContentProperty);
        }

        /// <summary>
        ///     Sets the On content for the <see cref="ToggleSwitchButton" />
        /// </summary>
        /// <param name="element"></param>
        /// <param name="content">The content</param>
        public static void SetOnContent(UIElement element, object content)
        {
            element.SetValue(OnContentProperty, content);
        }

        /// <summary>
        ///     Gets the Off brush foreground color for the <see cref="ToggleSwitchButton" />
        /// </summary>
        /// <param name="element"></param>
        /// <returns>The color brush</returns>
        public static Brush GetOffBrush(UIElement element)
        {
            return (Brush)element.GetValue(OffBrushProperty);
        }

        /// <summary>
        ///     Sets the Off brush foreground color for the <see cref="ToggleSwitchButton" />
        /// </summary>
        /// <param name="element"></param>
        /// <param name="brush">The color brush</param>
        public static void SetOffBrush(UIElement element, Brush brush)
        {
            element.SetValue(OffBrushProperty, brush);
        }

        /// <summary>
        ///     Gets the On brush foreground color for the <see cref="ToggleSwitchButton" />
        /// </summary>
        /// <param name="element"></param>
        /// <returns>The color brush</returns>
        public static Brush GetOnBrush(UIElement element)
        {
            return (Brush)element.GetValue(OnBrushProperty);
        }

        /// <summary>
        ///     Sets the On brush foreground color for the <see cref="ToggleSwitchButton" />
        /// </summary>
        /// <param name="element"></param>
        /// <param name="brush">The color brush</param>
        public static void SetOnBrush(UIElement element, Brush brush)
        {
            element.SetValue(OnBrushProperty, brush);
        }

        private static void OnOffContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var button = sender as ToggleSwitch;
            if (button == null)
            {
                throw new InvalidOperationException(
                    $"The property 'OffContent' may only be set on {nameof(ToggleSwitch)} elements.");
            }
        }

        private static void OnOnContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var button = sender as ToggleSwitch;
            if (button == null)
            {
                throw new InvalidOperationException(
                    $"The property 'OnContent' may only be set on {nameof(ToggleSwitch)} elements.");
            }
        }

        private static void OnOffBrushChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var button = sender as ToggleSwitch;
            if (button == null)
            {
                throw new InvalidOperationException(
                    $"The property 'OffBrush' may only be set on {nameof(ToggleSwitch)} elements.");
            }
        }

        private static void OnOnBrushChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var button = sender as ToggleSwitch;
            if (button == null)
            {
                throw new InvalidOperationException(
                    $"The property 'OnBrush' may only be set on {nameof(ToggleSwitch)} elements.");
            }
        }
    }
}