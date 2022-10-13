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
        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoWindow" /> class
        /// </summary>
        public InfoWindow()
        {
            InitializeComponent();

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
    }
}
