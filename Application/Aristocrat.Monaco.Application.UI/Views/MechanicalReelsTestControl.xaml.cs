namespace Aristocrat.Monaco.Application.UI.Views
{
    using System.Reflection;
    using System.Windows;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;
    using Simulation.HarkeyReels;
    using Simulation.HarkeyReels.Controls;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for MechanicalReelsTestControl.xaml
    /// </summary>
    public partial class MechanicalReelsTestControl
    {
        private const string GamesPath = "/Games";
        private const string PackagesPath = "/Packages";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelController _reelController;
        private readonly string _gamePath;
        private readonly string _packagesPath;

        public MechanicalReelsTestControl()
        {
            InitializeComponent();

            var pathMapper = ServiceManager.GetInstance().GetService<IPathMapper>();
            _reelController = ServiceManager.GetInstance().GetService<IReelController>();
            _gamePath = pathMapper.GetDirectory(GamesPath).FullName;
            _packagesPath = pathMapper.GetDirectory(PackagesPath).FullName;
        }

        public void ControlLoaded(object sender, RoutedEventArgs e)
        {
            // Hook up the data context, by finding the page's appropriate VM.
            var parent = Parent;
            while (!(parent is MechanicalReelsPage))
            {
                parent = ((FrameworkElement)parent).Parent;
            }

            if (((MechanicalReelsPage)parent).DataContext is not MechanicalReelsPageViewModel parentViewModel)
            {
                return;
            }

            if (_reelController?.HasCapability<IReelSpinCapabilities>() ?? false)
            {
                // Harkey Simulator
                DataContext = parentViewModel.ReelTestViewModel;
                parentViewModel.ReelTestViewModel.ReelsSimulation = GetHarkeySimulator();
                Simulation.HarkeyReels.Logger.Log += SimulatorLog;
            }
            else
            {
                // RELM Simulator
            }
        }

        private IReelDisplayControl GetHarkeySimulator()
        {
            var reelCount = _reelController?.ConnectedReels.Count ?? 0;
            var reelsControl = new ReelSetControl(_gamePath, reelCount, _packagesPath);

            ReelsWrapper.Child = reelsControl;
            return reelsControl.ReelDisplayControl;
        }

        private void SimulatorLog(object sender, LoggingEventArgs e)
        {
            if (e.IsError)
            {
                Logger.Error($"{sender}: {e.Text}");
            }
            else
            {
                Logger.Debug($"{sender}: {e.Text}");
            }
        }
    }
}