namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for ChooserView.xaml
    /// </summary>
    public partial class ChooserView
    {
        public ChooserView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetService<ChooserViewModel>();
        }
    }
}
