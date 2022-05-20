namespace Aristocrat.Monaco.Localization.Markup
{
    using System.Windows;
    using XAMLMarkupExtensions.Base;

    /// <summary>
    ///     Contains attachable properties localization.
    /// </summary>
    public class Localizer : DependencyObject
    {
        /// <summary>
        ///     <see cref="DependencyProperty"/> set the culture provider.
        /// </summary>
        public static readonly DependencyProperty ForProperty =
            DependencyProperty.RegisterAttached(
                "For",
                typeof(string),
                typeof(Localizer),
                new FrameworkPropertyMetadata(default(string),
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///     <see cref="DependencyProperty"/> set a value that determines whether overrides will be used.
        /// </summary>
        public static readonly DependencyProperty AllowOverridesProperty =
            DependencyProperty.RegisterAttached(
                "AllowOverrides",
                typeof(bool),
                typeof(Localizer),
                new FrameworkPropertyMetadata(true,
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior));

        /// <summary>
        ///     <see cref="DependencyProperty"/> set a value that determines if the target element will be aware of localization changes.
        /// </summary>
        public static readonly DependencyProperty LocalizationAwareProperty =
            DependencyProperty.RegisterAttached(
                "LocalizationAware",
                typeof(bool),
                typeof(Localizer),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.None, OnLocalizeAwareChanged));

        private static void OnLocalizeAwareChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue)
            {
                LocalizationEventManager.AddTarget(target);
            }
            else
            {
                LocalizationEventManager.RemoveTarget(target);
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="ForProperty"/> value.
        /// </summary>
        public string For
        {
            get => (string)GetValue(ForProperty);

            set => SetValue(ForProperty, value);
        }

        /// <summary>
        ///     Gets or sets the <see cref="AllowOverridesProperty"/> value.
        /// </summary>
        public bool AllowOverrides
        {
            get => (bool)GetValue(AllowOverridesProperty);

            set => SetValue(AllowOverridesProperty, value);
        }

        /// <summary>
        ///     Gets or sets the <see cref="LocalizationAwareProperty"/> value.
        /// </summary>
        public bool LocalizationAware
        {
            get => (bool)GetValue(LocalizationAwareProperty);

            set => SetValue(LocalizationAwareProperty, value);
        }

        /// <summary>
        ///     Getter of <see cref="ForProperty"/>.
        /// </summary>
        /// <param name="obj">The target dependency object.</param>
        /// <returns>The value of the property.</returns>
        public static string GetFor(DependencyObject obj)
        {
            return obj?.GetValueSync<string>(ForProperty);
        }

        /// <summary>
        ///     Setter of <see cref="ForProperty"/>.
        /// </summary>
        /// <param name="obj">The target dependency object.</param>
        /// <param name="value">The value of the property.</param>
        public static void SetFor(DependencyObject obj, string value)
        {
            obj?.SetValueSync(ForProperty, value);
        }

        /// <summary>
        ///     Getter of <see cref="AllowOverridesProperty"/>.
        /// </summary>
        /// <param name="obj">The target dependency object.</param>
        /// <returns>The value of the property.</returns>
        public static bool GetAllowOverrides(DependencyObject obj)
        {
            return obj?.GetValueSync<bool>(AllowOverridesProperty) ?? true;
        }

        /// <summary>
        ///     Setter of <see cref="AllowOverridesProperty"/>.
        /// </summary>
        /// <param name="obj">The target dependency object.</param>
        /// <param name="value">The value of the property.</param>
        public static void SetAllowOverrides(DependencyObject obj, bool value)
        {
            obj?.SetValueSync(AllowOverridesProperty, value);
        }

        /// <summary>
        ///     Getter of <see cref="LocalizationAwareProperty"/>.
        /// </summary>
        /// <param name="obj">The target dependency object.</param>
        /// <returns>The value of the property.</returns>
        public static bool GetLocalizationAware(DependencyObject obj)
        {
            return obj?.GetValueSync<bool>(LocalizationAwareProperty) ?? false;
        }

        /// <summary>
        ///     Setter of <see cref="LocalizationAwareProperty"/>.
        /// </summary>
        /// <param name="obj">The target dependency object.</param>
        /// <param name="value">The value of the property.</param>
        public static void SetLocalizationAware(DependencyObject obj, bool value)
        {
            obj?.SetValueSync(LocalizationAwareProperty, value);
        }
    }
}
