namespace Aristocrat.Monaco.Gaming.UI.Views.Lobby
{
    using System;
    using System.Windows;
    using MahApps.Metro.Controls;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow
    {
        private readonly MetroWindow _parent;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoWindow" /> class
        /// </summary>
        /// <param name="parent">Parent window</param>
        public InfoWindow(MetroWindow parent)
        {
            InitializeComponent();

            // MetroApps issue--need to set in code behind after InitializeComponent.
            AllowsTransparency = true;

            _parent = parent;

            IsVisibleChanged += InfoWindow_IsVisibleChanged;

            DataContext = new InfoOverlayViewModel();
        }

        /// <summary>
        ///     Gets or sets the view model for the view.
        /// </summary>
        public InfoOverlayViewModel ViewModel
        {
            get => DataContext as InfoOverlayViewModel;
            set => DataContext = value;
        }

        private void InfoWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;
            if (isVisible)
            {
                Top = _parent.Top;
                Left = _parent.Left;

                Width = _parent.Width;
                Height = _parent.Height;

                WindowState = _parent.WindowState;
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            ViewModel?.Dispose();
            ViewModel = null;
        }
    }
}
