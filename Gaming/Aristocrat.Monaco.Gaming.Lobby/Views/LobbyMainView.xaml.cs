namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for LobbyMainView.xaml
    /// </summary>
    public partial class LobbyMainView
    {
        public LobbyMainView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetService<LobbyMainViewModel>();

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
