namespace Aristocrat.Monaco.UI.Common.Events
{
    using System.Windows;
    using System.Windows.Interactivity;

    /// <summary>
    ///     Used to route attached events to command
    /// </summary>
    public class RoutedEventTrigger : EventTriggerBase<FrameworkElement>
    {
        /// <summary>
        ///     <see cref="DependencyProperty" /> set a value that determines if the target element will be aware of localization
        ///     changes.
        /// </summary>
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.RegisterAttached(
                "Target",
                typeof(FrameworkElement),
                typeof(RoutedEventTrigger),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.None));

        /// <summary>
        ///     Routed event
        /// </summary>
        public RoutedEvent RoutedEvent { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="TargetProperty" /> value.
        /// </summary>
        public FrameworkElement Target
        {
            get => (FrameworkElement)GetValue(TargetProperty);

            set => SetValue(TargetProperty, value);
        }

        /// <summary>
        ///     Getter of <see cref="TargetProperty" />.
        /// </summary>
        /// <param name="obj">The target dependency object.</param>
        /// <returns>The value of the property.</returns>
        public static FrameworkElement GetTarget(DependencyObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (!obj.Dispatcher.CheckAccess())
            {
                return (FrameworkElement)obj.Dispatcher.Invoke(() => obj.GetValue(TargetProperty));
            }

            return (FrameworkElement)obj.GetValue(TargetProperty);
        }

        /// <summary>
        ///     Setter of <see cref="TargetProperty" />.
        /// </summary>
        /// <param name="obj">The target dependency object.</param>
        /// <param name="value">The value of the property.</param>
        public static void SetTarget(DependencyObject obj, string value)
        {
            if (obj == null)
            {
                return;
            }

            if (!obj.Dispatcher.CheckAccess())
            {
                obj.Dispatcher.Invoke(() => obj.SetValue(TargetProperty, value));
            }
            else
            {
                obj.SetValue(TargetProperty, value);
            }
        }

        /// <summary>
        ///     Attach the routed event to the command
        /// </summary>
        protected override void OnAttached()
        {
            if (RoutedEvent == null)
            {
                return;
            }

            if (Target == null)
            {
                if (AssociatedObject is Behavior behavior)
                {
                    Target = ((IAttachedObject)behavior).AssociatedObject as FrameworkElement;
                }

                Target = AssociatedObject as FrameworkElement;
            }

            Target?.AddHandler(RoutedEvent, new RoutedEventHandler(OnRoutedEvent));
        }

        /// <summary>
        ///     Get the routed event name
        /// </summary>
        protected override string GetEventName()
        {
            return RoutedEvent.Name;
        }

        private void OnRoutedEvent(object sender, RoutedEventArgs args)
        {
            OnEvent(args);
        }
    }
}