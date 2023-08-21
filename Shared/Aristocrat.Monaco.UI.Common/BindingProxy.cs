namespace Aristocrat.Monaco.UI.Common
{
    using System.Windows;

    /// <summary>
    ///     Binding Proxy for data grids
    /// </summary>
    /// <inheritdoc />
    public class BindingProxy : Freezable
    {
        /// <summary>
        ///     Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the Data for the binding
        /// </summary>
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <inheritdoc />
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}