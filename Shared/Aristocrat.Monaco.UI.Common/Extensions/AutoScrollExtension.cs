namespace Aristocrat.Monaco.UI.Common.Extensions
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     Helper class for views.
    /// </summary>
    public static class AutoScrollExtension
    {
        /// <summary>
        ///     Attached Auto Scroll property.
        /// </summary>
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScroll",
                typeof(bool),
                typeof(AutoScrollExtension),
                new PropertyMetadata(false, AutoScrollPropertyChanged));

        /// <summary>
        ///     Gets Auto Scroll attached property for specified object.
        /// </summary>
        /// <param name="obj">Dependency object.</param>
        /// <returns>Returns Auto Scroll attached property.</returns>
        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        /// <summary>
        ///     Sets Auto Scroll attached property for specified object.
        /// </summary>
        /// <param name="obj">Dependency object.</param>
        /// <param name="value">Indicates either Auto Scroll is 'on' or not.</param>
        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }

        private static void AutoScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer && (bool)e.NewValue)
            {
                scrollViewer.ScrollToBottom();
            }
        }
    }
}