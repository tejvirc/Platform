namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for LoadingMainView.xaml
    /// </summary>
    public partial class LoadingMainView
    {
        public LoadingMainView()
        {
            InitializeComponent();

            // DataContext = Application.Current.GetObject<LoadingMainViewModel>();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
