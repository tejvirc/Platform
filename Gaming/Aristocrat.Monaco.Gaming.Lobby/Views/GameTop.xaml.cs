namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for GameTop.xaml
    /// </summary>
    public partial class GameTop
    {
        public GameTop()
        {
            InitializeComponent();

            DataContext = Application.Current.GetService<GameTopViewModel>();
        }
    }
}
