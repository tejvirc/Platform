namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    /// <summary>
    /// Interaction logic for DefaultLobbyView.xaml
    /// </summary>
    public partial class DefaultLobbyView
    {
        public DefaultLobbyView(object viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}
