namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for GameMainView.xaml
    /// </summary>
    public partial class GameMainView
    {
        public GameMainView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<GameMainViewModel>();
        }
    }
}
