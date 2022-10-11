namespace Aristocrat.Monaco.Gaming.UI.Behaviors
{
    using System;
    using System.Windows;
    using Microsoft.Xaml.Behaviors;

    /// <summary>
    ///     MVVM helper to Activate a window with ViewModel.
    /// </summary>
    public class ActivateBehavior : Behavior<Window>
    {
        /// <summary>  Activated DependencyProperty </summary>
        public static readonly DependencyProperty ActivatedProperty =
            DependencyProperty.Register(
                "Activated",
                typeof(bool),
                typeof(ActivateBehavior),
                new PropertyMetadata(OnActivatedChanged));

        private bool _isActivated;

        /// <summary>
        ///     Gets or sets a value indicating whether the window is activated or not.
        /// </summary>
        public bool Activated
        {
            get => (bool)GetValue(ActivatedProperty);
            set => SetValue(ActivatedProperty, value);
        }

        /// <summary> Attach event handlers. </summary>
        protected override void OnAttached()
        {
            AssociatedObject.Activated += OnActivated;
            AssociatedObject.Deactivated += OnDeactivated;
        }

        /// <summary> Detach event handlers. </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.Activated -= OnActivated;
            AssociatedObject.Deactivated -= OnDeactivated;
        }

        private static void OnActivatedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (ActivateBehavior)dependencyObject;

            if (!behavior.Activated || behavior._isActivated)
            {
                return;
            }

            // The Activated property is set to true but the Activated event (tracked by the
            // isActivated field) hasn't been fired. Go ahead and activate the window.
            if (behavior.AssociatedObject.WindowState == WindowState.Minimized)
            {
                behavior.AssociatedObject.WindowState = WindowState.Normal;
            }

            behavior.AssociatedObject.Activate();
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            _isActivated = true;
            Activated = true;
        }

        private void OnDeactivated(object sender, EventArgs eventArgs)
        {
            _isActivated = false;
            Activated = false;
        }
    }
}
