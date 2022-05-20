namespace Aristocrat.Monaco.UI.Common.Behaviors
{
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Interactivity;

    /// <summary>
    ///     Attached behavior for multiple selection controls
    /// </summary>
    public class MultiSelectionBehavior : Behavior<Selector>
    {
        /// <summary>
        ///     Dependency property for selected items
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
                "SelectedItems",
                typeof(IList),
                typeof(MultiSelectionBehavior),
                new FrameworkPropertyMetadata(default(IList), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///     Get or sets the selected items
        /// </summary>
        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);

            set => SetValue(SelectedItemsProperty, value);
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null)
            {
                return;
            }

            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (SelectedItems == null)
            {
                return;
            }

            foreach (var item in args.RemovedItems)
            {
                SelectedItems.Remove(item);
            }

            foreach (var item in args.AddedItems)
            {
                SelectedItems.Add(item);
            }
        }
    }
}