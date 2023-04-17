namespace Aristocrat.Monaco.Application.UI.Views
{
    using System.Reflection;
    using System.Windows;
    using Hardware.Contracts.Reel;
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
        private readonly IReelDisplayControl _reelDisplayControl;

        public MechanicalReelsTestControl()
        {
            InitializeComponent();

            // Populate the external reels simulation control.
            var pathMapper = ServiceManager.GetInstance().GetService<IPathMapper>();
            var reelCount = ServiceManager.GetInstance().GetService<IReelController>()
                .ConnectedReels.Count;

            var gamePath = pathMapper.GetDirectory(GamesPath).FullName;
            var packagesPath = pathMapper.GetDirectory(PackagesPath).FullName;

            Simulation.HarkeyReels.Logger.Log += SimulatorLog;
            var reelsControl = new ReelSetControl(gamePath, reelCount, packagesPath);

            ReelsWrapper.Child = reelsControl;

            _reelDisplayControl = reelsControl.ReelDisplayControl;
        }

        public void ControlLoaded(object sender, RoutedEventArgs e)
        {
            // Hook up the data context, by finding the page's appropriate VM.
            var parent = Parent;
            while (!(parent is MechanicalReelsPage))
            {
                parent = ((FrameworkElement)parent).Parent;
            }
            if (((MechanicalReelsPage)parent).DataContext is MechanicalReelsPageViewModel parentVm)
            {
                DataContext = parentVm.ReelTestViewModel;

                // Hook up the VM to the reel display control.
                parentVm.ReelTestViewModel.ReelsSimulation = _reelDisplayControl;
            }
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