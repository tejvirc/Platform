namespace Aristocrat.Monaco.Application.UI.Views
{
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Kernel;
    using log4net;
    using Simulation.HarkeyReels;
    using Simulation.HarkeyReels.Controls;
    using ViewModels;

    /// <summary>
    /// Interaction logic for MechanicalReelsAnimationTestControl.xaml
    /// </summary>
    public partial class MechanicalReelsAnimationTestControl : UserControl
    {
        private const string GamesPath = "/Games";
        private const string PackagesPath = "/Packages";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelController _reelController;
        private readonly string _gamePath;
        private readonly string _packagesPath;

        public MechanicalReelsAnimationTestControl()
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
            while (parent is not MechanicalReelsPage)
            {
                parent = ((FrameworkElement)parent).Parent;
            }

            if (((MechanicalReelsPage)parent).DataContext is not MechanicalReelsPageViewModel parentViewModel)
            {
                return;
            }

            // RELM Simulator
        }

        // TODO: make this open the relm simulator
        private IReelDisplayControl GetRelmSimulator()
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
