namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for GameMain.xaml
    /// </summary>
    public partial class GameMain
    {
        public GameMain()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<GameMainViewModel>();
        }
    }
}
