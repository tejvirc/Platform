namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for IdleTextView.xaml
    /// </summary>
    public partial class IdleTextView
    {
        public IdleTextView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<IdleTextViewModel>();
        }
    }
}
