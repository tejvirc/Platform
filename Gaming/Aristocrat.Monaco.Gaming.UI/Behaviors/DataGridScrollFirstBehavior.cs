namespace Aristocrat.Monaco.Gaming.UI.Behaviors
{
    using System.Windows;
    using System.Windows.Controls;
    using Monaco.UI.Common;

    public class DataGridScrollFirstBehavior
    {
        public static readonly DependencyProperty ScrollIntoViewProperty =
            DependencyProperty.RegisterAttached(
                "ScrollIntoView",
                typeof(bool),
                typeof(DataGridScrollFirstBehavior),
                new PropertyMetadata(
                    false,
                    (o, e) =>
                    {
                        if (!(e.NewValue is bool scrollToTop) || !scrollToTop || !(o is DataGrid dataGrid))
                        {
                            return;
                        }

                        var scrollViewer = TreeHelper.FindChild<ScrollViewer>(dataGrid);
                        scrollViewer?.ScrollToTop();
                    }));

        public static void SetScrollIntoView(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollIntoViewProperty, value);
        }
    }
}