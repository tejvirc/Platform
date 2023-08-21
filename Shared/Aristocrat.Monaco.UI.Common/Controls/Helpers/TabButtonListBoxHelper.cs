namespace Aristocrat.Monaco.UI.Common.Controls.Helpers
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     Control helper class for TabButtonListBox.
    /// </summary>
    public static class TabButtonListBoxHelper
    {
        /// <summary>
        ///     <see cref="DependencyProperty"/> for TabButtonListBoxItem header.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.RegisterAttached("Header", typeof(object), typeof(TabButtonListBoxHelper),
                new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.None));

        /// <summary>
        ///     Setter for <see cref="HeaderProperty"/>.
        /// </summary>
        [Category("Content")]
        [AttachedPropertyBrowsableForType(typeof(ContentControl))]
        public static object GetHeader(ContentControl control)
        {
            return control?.GetValue(HeaderProperty);
        }

        /// <summary>
        ///     Setter for <see cref="HeaderProperty"/>.
        /// </summary>
        public static void SetHeader(ContentControl control, object value)
        {
            control?.SetValue(HeaderProperty, value);
        }

        /// <summary>
        ///     <see cref="DependencyProperty"/> for TabButtonListBoxItem header template.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.RegisterAttached("HeaderTemplate", typeof(DataTemplate), typeof(TabButtonListBoxHelper),
                new FrameworkPropertyMetadata(default(DataTemplate), FrameworkPropertyMetadataOptions.None));

        /// <summary>
        ///     Setter for <see cref="HeaderTemplateProperty"/>.
        /// </summary>
        [Category("Content")]
        [AttachedPropertyBrowsableForType(typeof(ContentControl))]
        public static object GetHeaderTemplate(ContentControl control)
        {
            return control?.GetValue(HeaderTemplateProperty);
        }

        /// <summary>
        ///     Setter for <see cref="HeaderTemplateProperty"/>.
        /// </summary>
        public static void SetHeaderTemplate(ContentControl control, object value)
        {
            control?.SetValue(HeaderTemplateProperty, value);
        }
    }
}
