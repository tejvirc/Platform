namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for DefaultLobbyView.xaml
    /// </summary>
    public partial class DefaultLobbyView
    {
        public DefaultLobbyView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetService<DefaultLobbyViewModel>();
        }
    }
}
