namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System;
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for GameTopper.xaml
    /// </summary>
    public partial class GameTopper
    {
        public GameTopper()
        {
            InitializeComponent();

            DataContext = Application.Current.GetService<GameTopperViewModel>();
        }
    }
}
