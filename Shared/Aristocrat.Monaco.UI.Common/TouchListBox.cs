namespace Aristocrat.Monaco.UI.Common
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <inheritdoc />
    public class TouchListBox : ListBox
    {
        /// <inheritdoc />
        public TouchListBox()
        {
            TouchDown += OnTouchDown;
            TouchUp += OnTouchUp;
        }

        private static void OnTouchUp(object sender, TouchEventArgs e)
        {
            e.Handled = true;
        }

        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            var touchPosition = e.GetTouchPoint(this);

            var element = InputHitTest(touchPosition.Position) as UIElement;

            while (element != null && !element.Equals(this))
            {
                if (element is ListBoxItem item)
                {
                    item.IsSelected = true;
                    break;
                }

                element = VisualTreeHelper.GetParent(element) as UIElement;
            }

            e.Handled = true;
        }
    }
}