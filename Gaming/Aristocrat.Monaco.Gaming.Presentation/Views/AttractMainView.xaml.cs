namespace Aristocrat.Monaco.Gaming.Presentation.Views
{
    using Presentation.ViewModels;
    using UI.ViewModels;
    using Aristocrat.MVVM.ViewModel;

    using System.Windows;
    using Aristocrat.Monaco.Gaming.UI.Views.Overlay;
    using Prism.Regions;
    using Prism.Common;
    using System.ComponentModel;
    using Aristocrat.Monaco.Gaming.Presentation.Events;

    /// <summary>
    /// Interaction logic for AttractMainView.xaml
    /// </summary>
    public partial class AttractMainView
    {
        public AttractMainView()
        {
            InitializeComponent();
            ViewModel = new AttractMainViewModel();

            //RegionManager.GetObservableRegion(AttractRegion)
            //    .PropertyChanged += OnRegionObservableChanged;

            //LayoutTemplate.SizeChanged += (o, args) =>
            //{
            //    GameAttract.Height = args.NewSize.Height;
            //    GameAttract.Width = args.NewSize.Width;
            //};
        }

        public AttractMainViewModel ViewModel
        {
            get;

            set;
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

        private void GameAttract_OnVideoCompleted(object sender, RoutedEventArgs e)
        {
            ViewModel.OnGameAttractVideoCompleted();
        }
    }
}
