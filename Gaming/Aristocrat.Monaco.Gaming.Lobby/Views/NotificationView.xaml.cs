namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for NotificationView.xaml
    /// </summary>
    public partial class NotificationView
    {
        public NotificationView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<NotificationViewModel>();
        }
    }
}
