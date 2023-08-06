namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.Windows;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Application.UI.Views;
    using ViewModels;

    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell
    {
        private readonly WindowToScreenMapper _windowToScreenMapper = new(DisplayRole.Main);

        public Shell()
        {
            InitializeComponent();

            DataContext = Application.Current.GetObject<ShellViewModel>();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _windowToScreenMapper.MapWindow(this);

            ShowTitleBar = !_windowToScreenMapper.IsFullscreen;
            ShowCloseButton = !_windowToScreenMapper.IsFullscreen;
            ShowMinButton = !_windowToScreenMapper.IsFullscreen;
            ShowMaxRestoreButton = !_windowToScreenMapper.IsFullscreen;
        }
    }
}
