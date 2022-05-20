namespace Aristocrat.Monaco.UI.Common.Controls.Helpers
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     Control helper class for <see cref="CheckBoxHelper" />
    /// </summary>
    public static class CheckBoxHelper
    {
        /// <summary>
        /// </summary>
        public static readonly DependencyProperty CheckBoxPartWidthProperty
            = DependencyProperty.RegisterAttached(
                "CheckBoxPartWidth",
                typeof(int),
                typeof(CheckBoxHelper),
                new FrameworkPropertyMetadata(
                    30,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// </summary>
        public static readonly DependencyProperty CheckMarkPartWidthProperty
            = DependencyProperty.RegisterAttached(
                "CheckMarkPartWidth",
                typeof(int),
                typeof(CheckBoxHelper),
                new FrameworkPropertyMetadata(
                    30,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// </summary>
        public static readonly DependencyProperty CheckMarkPartHeightProperty
            = DependencyProperty.RegisterAttached(
                "CheckMarkPartHeight",
                typeof(int),
                typeof(CheckBoxHelper),
                new FrameworkPropertyMetadata(
                    30,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(CheckBox))]
        public static int GetCheckBoxPartWidth(UIElement element)
        {
            return (int)element.GetValue(CheckBoxPartWidthProperty);
        }

        /// <summary>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(CheckBox))]
        public static void SetCheckBoxPartWidth(UIElement element, int value)
        {
            element.SetValue(CheckBoxPartWidthProperty, value);
        }

        /// <summary>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(CheckBox))]
        public static int GetCheckMarkPartWidth(UIElement element)
        {
            return (int)element.GetValue(CheckMarkPartWidthProperty);
        }

        /// <summary>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(CheckBox))]
        public static void SetCheckMarkPartWidth(UIElement element, int value)
        {
            element.SetValue(CheckMarkPartWidthProperty, value);
        }

        /// <summary>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(CheckBox))]
        public static int GetCheckMarkPartHeight(UIElement element)
        {
            return (int)element.GetValue(CheckMarkPartHeightProperty);
        }

        /// <summary>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForType(typeof(CheckBox))]
        public static void SetCheckMarkPartHeight(UIElement element, int value)
        {
            element.SetValue(CheckMarkPartHeightProperty, value);
        }
    }
}