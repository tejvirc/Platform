namespace Aristocrat.Monaco.UI.Common.Controls.Helpers
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     helper for ListView
    /// </summary>
    public static class ListViewHelper
    {
        /// <summary>
        /// </summary>
        public static readonly DependencyProperty AllowsColumnReorderProperty
            = DependencyProperty.RegisterAttached(
                "AllowsColumnReorder",
                typeof(bool),
                typeof(ListViewHelper),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static bool GetAllowsColumnReorder(UIElement element)
        {
            return (bool)element.GetValue(AllowsColumnReorderProperty);
        }

        /// <summary>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static void SetAllowsColumnReorder(UIElement element, bool value)
        {
            element.SetValue(AllowsColumnReorderProperty, value);
        }
    }
}