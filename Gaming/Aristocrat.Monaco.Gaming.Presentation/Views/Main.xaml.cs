namespace Aristocrat.Monaco.Gaming.Presentation.Views
{
    using System.ComponentModel;
    using System.Windows;
    using Application.UI.Views;
    using Cabinet.Contracts;
    using Events;
    using Prism.Common;
    using Prism.Ioc;
    using Prism.Regions;

    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main
    {
        private readonly WindowToScreenMapper _windowToScreenMapper = new(DisplayRole.Main);

        public Main()
        {
            InitializeComponent();

            var regionManager = ContainerLocator.Current.Resolve<IRegionManager>();
            SetRegionManager(regionManager, MainRegion, RegionNames.Main);

            // DataContext = Application.Current.GetObject<ShellViewModel>();

            Loaded += OnLoaded;

            RegionManager.GetObservableRegion(MainRegion)
                .PropertyChanged += OnRegionObservableChanged;
        }

        public static readonly RoutedEvent RegionReadyEvent
           = EventManager.RegisterRoutedEvent(nameof(RegionReady),
                                              RoutingStrategy.Bubble,
                                              typeof(RegionReadyEventHandler),
                                              typeof(Main));

        public event RegionReadyEventHandler RegionReady
        {
            add => AddHandler(RegionReadyEvent, value);

            remove => RemoveHandler(RegionReadyEvent, value);
        }

        private void OnRegionObservableChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ObservableObject<IRegion> observable && e.PropertyName == nameof(ObservableObject<IRegion>.Value))
            {
                observable.Value.PropertyChanged += OnRegionChanged;
            }
        }

        private void OnRegionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is IRegion region && e.PropertyName == nameof(Region.RegionManager))
            {
                RaiseEvent(new RegionReadyEventArgs(RegionReadyEvent, this, region));
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _windowToScreenMapper.MapWindow(this);

            ShowTitleBar = !_windowToScreenMapper.IsFullscreen;
            ShowCloseButton = !_windowToScreenMapper.IsFullscreen;
            ShowMinButton = !_windowToScreenMapper.IsFullscreen;
            ShowMaxRestoreButton = !_windowToScreenMapper.IsFullscreen;
        }

        private static void SetRegionManager(IRegionManager regionManager, DependencyObject regionTarget, string regionName)
        {
            RegionManager.SetRegionName(regionTarget, regionName);
            RegionManager.SetRegionManager(regionTarget, regionManager);
        }
    }
}
