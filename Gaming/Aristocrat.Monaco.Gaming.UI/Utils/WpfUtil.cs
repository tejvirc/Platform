namespace Aristocrat.Monaco.Gaming.UI.Utils
{
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Miscellaneous WPF utilities.
    /// </summary>
    public static class WpfUtil
    {
        /// <summary>
        /// Binds the given view's dependency property to its associated viewmodel property.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="viewProp">The view's dependency property.</param>
        /// <param name="vm">The viewmodel.</param>
        /// <param name="vmPropName">The name of the viewmodel prroperty to be bound to <param name="viewProp"/>.</param>
        /// <param name="mode">The binding mode.</param>
        /// <param name="trigger">The binding's edge triggering.</param>
        public static void Bind(FrameworkElement view, DependencyProperty viewProp, object vm, string vmPropName,
            BindingMode mode, UpdateSourceTrigger trigger = UpdateSourceTrigger.PropertyChanged)
        {
            var binding = new Binding()
            {
                Source = vm,
                Path = new PropertyPath(vmPropName),
                Mode = mode,
                UpdateSourceTrigger = trigger
            };
            view.SetBinding(viewProp, binding);
        }
    }
}
