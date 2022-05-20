namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     CustomItemsControl.  Gets rid of the ContentPresenter.  Use this if you need to set ZIndex on items in the control
    /// </summary>
    public class CustomItemsControl : ItemsControl
    {
        /// <inheritdoc />
        protected override DependencyObject GetContainerForItemOverride()
        {
            return ItemTemplate?.LoadContent() ?? base.GetContainerForItemOverride();
        }

        /// <inheritdoc />
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ((FrameworkElement)element).DataContext = item;
        }
    }
}