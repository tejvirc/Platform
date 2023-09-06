namespace Aristocrat.Monaco.Gaming.Presentation.Views
{
    using System.ComponentModel;
    using System.Windows;
    using Events;
    using Prism.Common;
    using Prism.Regions;

    /// <summary>
    /// Interaction logic for LobbyMainView.xaml
    /// </summary>
    public partial class LobbyMainView
    {
        public LobbyMainView()
        {
            InitializeComponent();

            // DataContext = Application.Current.GetObject<LobbyMainViewModel>();

            RegionManager.GetObservableRegion(ChooserRegion)
                .PropertyChanged += OnRegionObservableChanged;

            RegionManager.GetObservableRegion(BannerRegion)
                .PropertyChanged += OnRegionObservableChanged;

            RegionManager.GetObservableRegion(UpiRegion)
                .PropertyChanged += OnRegionObservableChanged;

            RegionManager.GetObservableRegion(InfoBarRegion)
                .PropertyChanged += OnRegionObservableChanged;

            RegionManager.GetObservableRegion(ReplayNavRegion)
                .PropertyChanged += OnRegionObservableChanged;

            RegionManager.GetObservableRegion(PaidMeterRegion)
                .PropertyChanged += OnRegionObservableChanged;

            RegionManager.GetObservableRegion(NotificationRegion)
                .PropertyChanged += OnRegionObservableChanged;
        }

        public static readonly RoutedEvent RegionReadyEvent
           = EventManager.RegisterRoutedEvent(nameof(RegionReady),
                                              RoutingStrategy.Bubble,
                                              typeof(RegionReadyEventHandler),
                                              typeof(LobbyMainView));

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
    }
}
