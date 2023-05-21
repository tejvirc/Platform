namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for AttractMainView.xaml
    /// </summary>
    public partial class AttractMainView
    {
        public AttractMainView()
        {
            InitializeComponent();

            DataContext = Application.Current.GetService<AttractMainViewModel>();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
