namespace Aristocrat.Monaco.UI.Common.Behaviors
{
    using System.Windows;
    using Microsoft.Xaml.Behaviors;

    /// <summary>
    ///     Behavior to pump WM_Touch messages to a window that has had WPF Stylus Support turned off.
    /// </summary>
    public class TouchWindowBehavior : Behavior<Window>
    {
        private TouchWindowHelper _helper;
        /// <summary> Attach event handlers. </summary>
        protected override void OnAttached()
        {
            _helper = new TouchWindowHelper(AssociatedObject);
            AssociatedObject.Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object o, RoutedEventArgs e)
        {
            AssociatedObject.Unloaded -= OnUnloaded;
            if (_helper != null)
            {
                _helper.Dispose();
                _helper = null;
            }
        }
    }
}