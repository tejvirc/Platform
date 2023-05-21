namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for ReplayNavView.xaml
    /// </summary>
    public partial class ReplayNavView
    {
        public ReplayNavView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetService<ReplayNavViewModel>();
        }
    }
}
