namespace Aristocrat.Monaco.UI.Common.Extensions
{
    using System.Windows;

    /// <summary>
    ///     Focus Extension
    /// </summary>
    public class FocusExtension : DependencyObject
    {
        /// <summary>
        ///     The is focused property
        /// </summary>
        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached(
            "IsFocused",
            typeof(bool),
            typeof(FocusExtension),
            new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        /// <summary>
        ///     Gets or sets value indicating whether the object is focused
        /// </summary>
        public bool IsFocused
        {
            get => (bool)GetValue(IsFocusedProperty);

            set => SetValue(IsFocusedProperty, value);
        }

        /// <summary>
        ///     Gets the is focused.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>True is focused</returns>
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        /// <summary>
        ///     Sets the is focused.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        private static void OnIsFocusedPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;
            if ((bool)e.NewValue)
            {
                uie.Focus(); // Don't care about false values.
            }
        }
    }
}