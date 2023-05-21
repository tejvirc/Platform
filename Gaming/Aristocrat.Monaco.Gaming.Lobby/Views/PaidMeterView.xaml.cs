namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for PaidMeterView.xaml
    /// </summary>
    public partial class PaidMeterView
    {
        public PaidMeterView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetService<PaidMeterViewModel>();
        }
    }
}
