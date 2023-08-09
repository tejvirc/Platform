namespace Aristocrat.Monaco.Gaming.Lobby.Views
{
    using System.ComponentModel;
    using System.Windows;
    using Application.UI.Views;
    using Cabinet.Contracts;

    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell
    {
        private readonly WindowToScreenMapper _windowToScreenMapper = new(DisplayRole.Main);

        public Shell()
        {
            InitializeComponent();

            // DataContext = Application.Current.GetObject<ShellViewModel>();

            Loaded += OnLoaded;

            var region = Prism.Regions.RegionManager.GetObservableRegion(ShellRegion);
            region.PropertyChanged += OnRegionObservableChanged;
        }

        private void OnRegionObservableChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not Prism.Common.ObservableObject<Prism.Regions.IRegion> observable)
            {
                return;
            }

            var region = observable.Value;

            region.PropertyChanged -= OnRegionChanged;
        }

        private void OnRegionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Prism.Regions.IRegion region)
            {
                return;
            }

            var propertyName = e.PropertyName;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _windowToScreenMapper.MapWindow(this);

            ShowTitleBar = !_windowToScreenMapper.IsFullscreen;
            ShowCloseButton = !_windowToScreenMapper.IsFullscreen;
            ShowMinButton = !_windowToScreenMapper.IsFullscreen;
            ShowMaxRestoreButton = !_windowToScreenMapper.IsFullscreen;
        }

        private void OnViewRegionClicked(object sender, RoutedEventArgs e)
        {
            var region = Prism.Regions.RegionManager.GetObservableRegion(ShellRegion).Value;
        }
    }
}
