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

            DataContext = Application.Current.GetObject<LobbyMainViewModel>();
        }
    }
}
