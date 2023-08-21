namespace Aristocrat.Monaco.UI.Common
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     ImageHelper class allows you to set up a DependencyProperty on an Image
    ///     to dynamically bind to resources
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        ///     SourceResourceKeyProperty is the dependency property to add to the Image
        /// </summary>
        public static readonly DependencyProperty SourceResourceKeyProperty = DependencyProperty.RegisterAttached(
            "SourceResourceKey",
            typeof(object),
            typeof(ImageHelper),
            new PropertyMetadata(string.Empty, SourceResourceKeyChanged));

        private static void SourceResourceKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Image element && e.NewValue != null)
            {
                element.SetResourceReference(Image.SourceProperty, e.NewValue);
            }
        }

        /// <summary>
        ///     SetSourceResourceKey is called to set the ResourceKey
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetSourceResourceKey(Image element, object value)
        {
            element.SetValue(SourceResourceKeyProperty, value);
        }

        /// <summary>
        ///     GetSourceResourceKey is called to get the ResourceKey
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static object GetSourceResourceKey(Image element)
        {
            return element.GetValue(SourceResourceKeyProperty);
        }
    }
}
