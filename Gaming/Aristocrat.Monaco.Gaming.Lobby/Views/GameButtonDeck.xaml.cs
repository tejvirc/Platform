namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for GameButtonDeck.xaml
    /// </summary>
    public partial class GameButtonDeck
    {
        public GameButtonDeck()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<GameButtonDeckViewModel>();
        }
    }
}
