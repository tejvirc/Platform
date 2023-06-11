namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for MultiLanguageUpiView.xaml
    /// </summary>
    public partial class MultiLanguageUpiView
    {
        public MultiLanguageUpiView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<MultiLanguageUpiViewModel>();
        }
    }
}
