namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for StandardUpiView.xaml
    /// </summary>
    public partial class StandardUpiView
    {
        public StandardUpiView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<StandardUpiViewModel>();
        }
    }
}
